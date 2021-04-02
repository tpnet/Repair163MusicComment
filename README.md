# Change163MusicMetaData
Change163MusicMetaData,修改mp3的comment信息，让网易云可以识别出来歌曲，本项目是一个Unity3D项目，可以打出各个端的程序包。

# 界面

![界面](https://gitee.com/tpnet/UPic/raw/master/uPic/20210402/Yfto5r.png)

# 用法

 - 输入歌曲文件夹的地址（支持拖放），会修改文件夹里面的全部歌曲
 - 点击修改，等待下面转换结果打印出来了即可

# 原理
用过api接口获取歌曲的信息，然后生成comment，设置到mp3的metadata里面，使得网易云可以识别到歌曲。