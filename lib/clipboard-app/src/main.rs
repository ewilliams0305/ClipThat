use std::ffi::CStr;
use std::os::raw::c_char;
use std::{thread, time::Duration};

#[cfg(target_os = "windows")]
#[link(name = "vc-clipboard", kind = "static")]
unsafe extern "C" {
    fn start_clipboard_monitor(callback: extern "C" fn(*const c_char));
    fn init_clipboard();
}

#[cfg(target_os = "linux")]
#[link(name = "vc-clipboard", kind = "static")]
unsafe extern "C" {
    fn start_clipboard_monitor(callback: extern "C" fn(*const c_char));
    fn init_clipboard();
}

// Define a callback function that prints clipboard updates
extern "C" fn clipboard_callback(text: *const c_char) {
    if text.is_null() {
        println!("Received NULL clipboard data");
        return;
    }

    // Convert C string (char*) to Rust string
    let c_str = unsafe { CStr::from_ptr(text) };
    let text_str = c_str.to_str().unwrap_or("<Invalid UTF-8>");
    println!("Clipboard changed: {}", text_str);
}

fn main() {
    println!("Initializing clipboard monitoring...");

    // Initialize the clipboard
    unsafe { init_clipboard() };

    // Start monitoring clipboard changes
    unsafe { start_clipboard_monitor(clipboard_callback) };

    println!("Monitoring clipboard... Press CTRL+C to stop.");
    loop {
        thread::sleep(Duration::from_secs(1));
    }
}
