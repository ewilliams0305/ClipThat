use clipboard::{ClipboardContext, ClipboardProvider};
use std::{
    ffi::CStr,
    ffi::CString,
    os::raw::c_char,
    sync::{
        atomic::{AtomicBool, Ordering},
        Arc, Mutex,
    },
    thread,
    time::Duration,
};

type ClipboardCallback = extern "C" fn(*const c_char);
type ErrorCallback = extern "C" fn(*const c_char);

// static CLIPBOARD: Mutex<Option<ClipboardContext>> = Mutex::new(None);
static CLIPBOARD_CONTEXT: Mutex<Option<ClipboardContext>> = Mutex::new(None);

static MONITOR_RUNNING: AtomicBool = AtomicBool::new(false);

#[no_mangle]
pub extern "C" fn set_clipboard_text(text: *const c_char) {
    if text.is_null() {
        eprintln!("set_clipboard_text received NULL pointer");
        return;
    }

    // Convert C string to Rust string
    let c_str = unsafe { CStr::from_ptr(text) };
    let text_str = match c_str.to_str() {
        Ok(s) => s,
        Err(_) => {
            eprintln!("Failed to convert clipboard text to UTF-8");
            return;
        }
    };

    // Set clipboard content
    let mut ctx: ClipboardContext = match ClipboardProvider::new() {
        Ok(ctx) => ctx,
        Err(err) => {
            eprintln!("Clipboard initialization failed: {:?}", err);
            return;
        }
    };

    if let Err(err) = ctx.set_contents(text_str.to_string()) {
        eprintln!("Failed to set clipboard text: {:?}", err);
    }
}

#[no_mangle]
pub extern "C" fn start_clipboard_monitor(
    clipboard_callback: ClipboardCallback,
    error_callback: ErrorCallback,
) {
    if MONITOR_RUNNING.load(Ordering::SeqCst) {
        let err_message = CString::new("Clipboard monitoring is already running.")
            .unwrap_or_else(|_| CString::new("Unknown error").unwrap());
        error_callback(err_message.as_ptr());
        return;
    }

    MONITOR_RUNNING.store(true, Ordering::SeqCst);

    let clipboard = match ClipboardContext::new() {
        Ok(ctx) => Arc::new(Mutex::new(ctx)),
        Err(err) => {
            let err_message = CString::new(format!("Failed to access clipboard: {:?}", err))
                .unwrap_or_else(|_| CString::new("Clipboard initialization error").unwrap());
            error_callback(err_message.as_ptr());
            return;
        }
    };

    let last_text = Arc::new(Mutex::new(String::new()));

    thread::spawn({
        let clipboard = Arc::clone(&clipboard);
        let last_text = Arc::clone(&last_text);

        move || loop {
            if !MONITOR_RUNNING.load(Ordering::SeqCst) {
                println!("Clipboard monitoring stopped.");
                break;
            }

            let clipboard_guard = clipboard.lock();
            let last_text_guard = last_text.lock();

            if let (Ok(mut clipboard), Ok(mut last_text)) = (clipboard_guard, last_text_guard) {
                match clipboard.get_contents() {
                    Ok(text) => {
                        if *last_text != text {
                            *last_text = text.clone();
                            if let Ok(c_text) = CString::new(text) {
                                clipboard_callback(c_text.as_ptr());
                            } else {
                                let err_msg =
                                    CString::new("Failed to convert clipboard text to CString")
                                        .unwrap_or_else(|_| {
                                            CString::new("Unknown CString error").unwrap()
                                        });
                                error_callback(err_msg.as_ptr());
                            }
                        }
                    }
                    Err(err) => {
                        let err_message = CString::new(format!("Clipboard error: {:?}", err))
                            .unwrap_or_else(|_| CString::new("Unknown clipboard error").unwrap());
                        error_callback(err_message.as_ptr());
                    }
                }
            } else {
                let err_msg = CString::new("Failed to acquire clipboard lock")
                    .unwrap_or_else(|_| CString::new("Unknown lock error").unwrap());
                error_callback(err_msg.as_ptr());
            }

            thread::sleep(Duration::from_millis(500));
        }
    });
}

#[no_mangle]
pub extern "C" fn poll_clipboard_text() -> *mut c_char {
    let mut clipboard_guard = match CLIPBOARD_CONTEXT.lock() {
        Ok(guarde) => guarde,
        Err(err) => {
            let err_message = format!("Clipboard initialization failed: {:?}", err);
            return CString::new(err_message).unwrap().into_raw();
        }
    };

    // Initialize the ClipboardContext lazily
    let clipboard = match *clipboard_guard {
        Some(ref mut ctx) => ctx, // Reuse the existing ClipboardContext
        None => {
            // If the context is not initialized, create it
            let ctx = match ClipboardProvider::new() {
                Ok(ctx) => ctx,
                Err(err) => {
                    let err_message = format!("Clipboard initialization failed: {:?}", err);
                    return CString::new(err_message).unwrap().into_raw();
                }
            };
            *clipboard_guard = Some(ctx); // Store it for future use
            clipboard_guard.as_mut().unwrap() // Return the newly created context
        }
    };

    match clipboard.get_contents() {
        Ok(text) => CString::new(text).unwrap().into_raw(),
        Err(err) => {
            let err_message = format!("Clipboard read failed: {:?}", err);
            return CString::new(err_message).unwrap().into_raw();
        }
    }
}

#[no_mangle]
pub extern "C" fn free_rust_string(s: *mut c_char) {
    if s.is_null() {
        return;
    }

    unsafe { drop(CString::from_raw(s)) };
}

#[no_mangle]
pub extern "C" fn init_clipboard() {
    let mut clipboard = match CLIPBOARD_CONTEXT.lock() {
        Ok(cb) => cb,
        Err(err) => {
            // Handle the error if locking the mutex fails
            eprintln!("Failed to lock clipboard context mutex: {:?}", err);
            return;
        }
    };

    match ClipboardContext::new() {
        Ok(ctx) => {
            *clipboard = Some(ctx);
        }
        Err(err) => {
            // Handle the error if clipboard initialization fails
            eprintln!("Failed to access clipboard: {:?}", err);
            return;
        }
    }
}

#[no_mangle]
pub extern "C" fn stop_clipboard_monitor() {
    MONITOR_RUNNING.store(false, Ordering::SeqCst);
    println!("Requested to stop clipboard monitoring.");
}
