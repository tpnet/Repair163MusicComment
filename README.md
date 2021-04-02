# Repair163MusicComment
Repair163MusicComment，修复修改mp3的Comment信息，让网易云可以识别出来别的地方下载的歌曲

# 界面

![界面](https://gitee.com/tpnet/UPic/raw/master/uPic/20210402/Yfto5r.png)

# 用法

 - 输入歌曲文件夹的地址（支持拖放）
 - 点击修改
 - 等待下面转换结果打印出来了即可

# 原理
用网易云音乐的api接口搜索歌曲，获取歌曲的信息，然后生成comment，再设置到mp3的metadata里面的comment，从而使得网易云可以识别到其他渠道下载的歌曲。