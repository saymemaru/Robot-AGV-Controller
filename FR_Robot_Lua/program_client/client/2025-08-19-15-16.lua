IP = "192.168.58.5"
port = 1145
name = "盖瑞"
while(1) do
    SocketOpen(IP,port,"socket_0")
    SocketSendString("我是" ..name,"socket_0",0)
    receivedMessage = SocketReadString("socket_0",0)
    if(receivedMessage == "start")then
        SetDO(1,1,0,0)
        WaitMs(5000)
        SetDO(1,0,0,0)
        SocketSendString("我好了","socket_0",0)
    end
    if(receivedMessage == "move")then
        
        SocketSendString("我好了","socket_0",0)
    end
    if(receivedMessage == "end")then
        SocketSendString("再见","socket_0",0)
        SocketClose("socket_0")
        WaitMs(5000)
    end
end
