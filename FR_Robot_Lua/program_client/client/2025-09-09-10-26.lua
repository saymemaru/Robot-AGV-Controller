IP = "192.168.58.5"
port = 1145
name = "盖瑞"
agvCode = "1"
mapCdoe = "2"

SocketOpen(IP,port,"socket_0")
SocketSendString("我是" ..name.."\n","socket_0",0)
while(1) do
    receivedMessage = SocketReadString("socket_0",0)
    if(receivedMessage == "start")then
        SetDO(1,1,0,0)
        WaitMs(5000)
        SetDO(1,0,0,0)
SocketSendString("我好了\n","socket_0",0)
    end
    if(receivedMessage == "move")then
SocketSendString("正在移动\n","socket_0",0)
        PTP(test2,100,500,0)
        PTP(pHome,100,500,0)
SocketSendString("我好了\n","socket_0",0)
    end
    if(receivedMessage == "work")then
SocketSendString("正在移动\n","socket_0",0)
        PTP(test2,100,500,0)
        PTP(pHome,100,500,0)
SocketSendString("我好了\n","socket_0",0)
sleep_ms(1000)
SocketSendString("/RecoverAGV"..agvName.." "..mapCdoe.."\n","socket_0",0)
    end   
    if(receivedMessage == "trygettime")then
SocketSendString("/time\n","socket_0",0)
    end
    if(receivedMessage == "end")then
SocketSendString("再见\n","socket_0",0)
        SocketClose("socket_0")
        WaitMs(5000)
    end
end