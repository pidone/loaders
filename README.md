# Loaders

This is a small project automate the creation of loaders.

## Prerequirements

* mono for C-Sharp / dotnet binaries
* msfvenom for the payload generation
* gettext-base for envsubst

### Kali
```sh
sudo apt install metasploit-framework mono-devel gettest-base
```

## Structure
There is a makefile to generate the payload with msfvenom and injected it into an template located at `templates`.
The source code will be located at `sources` and will be used to compile the loader.
The final loader will be written to `output`.

## Loader

### Classic

This is really the most basic loader. Just allocate RWX memory write the payload to it and start a new thread.
All modern AVs will detected this immediatly. For a couple of reasons:
* Known meterpreter payload
* RWX Memory region
* and so much more

```sh
make classic.exe
```

### Classic with AES

The loader is a small step further. Instead of baking the msfvenom payload directly
into the binary, it is encrypted with AES beforehand and decrypted at runtime.
In addition, the memory region is first made RW only and then excutable.
It is still recognized by practically all modern AVs, but outdated or very very
bad ones can probably already be bypassed.

The following things are still very suspicious:
* Entropy (the payload in the binary is at leased base64 decoded, which lowers the entropy)
* RW memory that is made excutable directly afterwards
* Known payload is in memory at runtime
* well-known injection method

```sh
make classic_aes.exe
```

### Process Injection

This loader uses an AES encrypted Payload and injects the decrypted payload in the give process (default: "explorer"),
creates a new thread for this process which points to the decrypted payload and run it.
This is still a well known method and get detected by nearly all AVs after 2022.

Suspicious Stuff:
* Entropy (the payload in the binary is at leased base64 decoded, which lowers the entropy)
* RW memory that is made excutable directly afterwards
* Known payload is in memory at runtime
* well-known injection method

Good Stuff:
* Word or some other process will no longer run strange commands
* The executated command and exit and the payload will keep alive

```sh
make process_inject.exe
```

### Jscript

This uses the process injection technique embedded in Jscript. It has a serialized dotnet assemble with JScript can
communicated via COM.

```sh
make dotnet.js
```
