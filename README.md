# Repair163MusicComment
Repair163MusicComment，一键修复修改mp3的Comment信息，让网易云可以识别出来别的地方下载的歌曲

# 界面

![界面](https://gitee.com/tpnet/UPic/raw/master/uPic/20210406/zO9J0q.png)


![加解密](https://gitee.com/tpnet/UPic/raw/master/uPic/20210406/8cK5pl.png)



# 功能
 - 一键批量修改增加mp3的Comment为网易云歌曲的Comment
 - 单首歌修改
 - 单首歌自定义网易云链接修改
 - 自动更新网易云数据库信息
 - 加解密Comment

# 用法

 - 输入歌曲 mp3文件/文件夹 的地址（支持拖放）
 - 点击修改
 - 等待下面转换结果打印出来了即可
 
 
# 原理
用网易云音乐的api接口搜索歌曲，匹配歌曲，得到歌曲的id，再使用api获取歌曲详情，然后根据详情信息生成网易云歌曲的Comment，设置到mp3的metadata里面，从而使得网易云可以识别到其他渠道下载的歌曲。