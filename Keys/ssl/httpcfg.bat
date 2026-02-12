rem Assign a thumbprint to an ssl ip:port
httpcfg set ssl -i 127.0.0.1:9001 -h 2148cb2a1f7b4f5ab871db5bbb0a134a221bc63c
httpcfg query ssl

rem on win7
netsh http add sslcert ipport=127.0.0.1:9002 certhash=2148cb2a1f7b4f5ab871db5bbb0a134a221bc63c appid={9d94beb0-7616-4bba-9536-b6ec1a97a30c}
netsh http show sslcert
