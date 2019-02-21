# CoreSolution
.NET Core下常用类库的封装
# 项目说明
### OneOne.Core.Cache
1. 缓存的帮助类
2. 目前集成了Couchbase缓存的操作，使用的是CouchbaseNetClient 2.7.4
3. 使用使用了ProtoBuf进行二进制序列化，同时进行了zip压缩

### OneOne.Core.IOC
1. 依赖注入封装类库
2. 集成了StructMap和AutoFac两种IOC框架

### OneOne.Core.Mq
1. MQ封装类库
2. 包含kafka的集成使用，消息序列化支持ProtoBuf和json两种

### OneOne.Core.Logger
1. 日志封装类库
2. 集成了Log4Net

### Web
1. WebAPi的demo类库
2. 使用了JWT的鉴权方式

### ConsoleTest
1. 测试Demo类库
<p>IocTest：IOC类库的demo类</p>
<p>CouchbaseTest：Cache类库的demo类</p>
<p>MqTest：Mq类库的demo类</p>


