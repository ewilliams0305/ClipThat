# VC Clipboard Monitor @RUST

The VC clipboard library is a background thread that monitors your clipboard, as text is copied it delgates this text to a function pointer. This function is linked to out C# library.

# Build Steps

- Install RUST https://www.rust-lang.org/learn/get-started
- Navigate to the `./vc-clipboard` directory
- Execute `cargo build --release` to generate a .dll located in the `./vc-clipboard/target/release` directory

Once compiled the library should be added to the C# code as an embedded resource and dynamically linked. See the C# solution for details.
