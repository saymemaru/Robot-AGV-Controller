while(1) do
    ip = "192.168.58.10"
    port = 114514
    SocketOpen(ip,port,"socket_0")
    SocketClose("socket_0")
    SocketSendString("hello"，"socket_0"，0))
    SocketReadString("socket_0",0)
    end