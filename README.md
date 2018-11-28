# SimpleBlog
![Beta](https://img.shields.io/badge/version-beta-red.svg)
[![MIT](https://img.shields.io/packagist/l/doctrine/orm.svg)](https://mit-license.org/)

Simple Blog 是一个使用 Asp.Net Core 开发的轻型跨平台博客系统。支持完整的自定义模板功能。可以自由上传任何静态Html页面作为模板，静态页面通过调用后台API获取数据。

## 已知问题
1.暂不支持RSS订阅，图片和附件上传以及博客编辑器；<br/>
2.使用火狐浏览后台，下拉菜单可能在弹出后无法关闭；<br/>
3.因为使用了ajax动态加载，搜索引擎可能无法正常爬取和收录，有待进一步优化。

## 快速开始
1.如果要开始使用，建议安装MySql数据库系统，并修改“Scottxu.Blog/appsettions.example.json”配置文件，并重命名为“Scottxu.Blog/appsettions.json”。<br/>
2.请手动创建“Scottxu.Blog/upload”文件夹，否则系统无法初始化。

## 联系作者
如果有任何问题请写Issus。<br/>
Email：scottxu@scottxublog.com

## 版权说明
本程序使用并遵循MIT开源许可，所引用的开源代码都在“Scottxu.Blog/wwwroot/sys/license.html”中声明。