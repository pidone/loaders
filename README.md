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


