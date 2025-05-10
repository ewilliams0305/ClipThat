using System.Runtime.InteropServices;

namespace ClipThat;

/// <summary>
/// Binds the RUST library in the ./lib directory to the C# library nuget package.
/// </summary>
#if NET8_0_OR_GREATER
internal static partial class ClipboardBindings
{
    private const string LibraryName = @"clipthat_clipboard";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ClipboardCallback(IntPtr text);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ErrorCallback(IntPtr errorMessage);


    [LibraryImport(LibraryName)]
    internal static partial void init_clipboard();


    [LibraryImport(LibraryName)]
    internal static partial void start_clipboard_monitor(ClipboardCallback callback, ErrorCallback errorCallback);


    [LibraryImport(LibraryName)]
    internal static partial void stop_clipboard_monitor();


    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void set_clipboard_text(string text);


    [LibraryImport(LibraryName)]
    internal static partial IntPtr poll_clipboard_text();


    [LibraryImport(LibraryName)]
    internal static partial void free_rust_string(IntPtr strPtr);

}
#else
internal static class ClipboardBindings
{
    private const string LibraryName = "clipthat";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ClipboardCallback(IntPtr text);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ErrorCallback(IntPtr errorMessage);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void init_clipboard();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void start_clipboard_monitor(ClipboardCallback callback, ErrorCallback errorCallback);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void stop_clipboard_monitor();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern void set_clipboard_text(string text);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr poll_clipboard_text();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void free_rust_string(IntPtr strPtr);
}
#endif