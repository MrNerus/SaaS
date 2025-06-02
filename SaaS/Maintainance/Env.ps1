// Following envs were used for testing locally
setx EncryptionKey "g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01g3O')/P#2%01"
setx ConfigConnection "{""Server"":""localhost"",""Port"":""5432"",""Database"":""configDb"",""Username"":""postgres"",""Password"":""postgres"",""Provider"":0}"
setx RedisConnection "{""Server"":""127.0.0.1"",""Port"":""6379"",""Provider"":3}"

[Environment]::SetEnvironmentVariable('EncryptionKey', [NullString]::Value, 'user')