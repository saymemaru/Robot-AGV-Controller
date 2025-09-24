IP = "192.168.58.5"
port = 1145

tcp = SocketOpen(IP,port,"socket_0")
SocketSendString("connected\n","socket_0",0)

while(1) do

    receivedMessage = SocketReadString("socket_0",0)
	
	--发送坐标，接收修正坐标
    if(receivedMessage == "getpose")then

        x,y,z,rx,ry,rz = GetActualTCPPose()
        poseString = string.format("%.4f %.4f %.4f %.4f %.4f %.4f", x,y,z,rx,ry,rz)
        SocketSendString("/getpose "..poseString.."\n","socket_0",0)
        
        --按空格读取处理后坐标
        receivedPoseString = SocketReadString("socket_0",0)
        resultPose = {}
        -- 使用模式匹配，%S+ 匹配一个或多个非空格字符
        for word in receivedPoseString:gmatch("[^%s]+") do
            local num = tonumber(word)
            --SocketSendString(num.."\n","socket_0",0)
            if num then  -- 只插入成功转换的数字
            table.insert(resultPose, num)
            end
        end    
        MoveCart(resultPose,0,0,100,50,50,0)
    end
	
end