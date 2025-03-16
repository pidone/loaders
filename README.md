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
make classic
```

### Classic with AES

The loader is a small step further. Instead of baking the msfvenom payload directly
into the binary, it is encrypted with AES beforehand and decrypted at runtime.
In addition, the memory region is first made RW only and then excutable.
It is still recognized by practically all modern AVs, but outdated or very very
bad ones can probably already be bypassed.

The following things are still very suspicious:
* Entropy
* RW memory that is made excutable directly afterwards
* Known payload is in memory at runtime
* well-known injection method



