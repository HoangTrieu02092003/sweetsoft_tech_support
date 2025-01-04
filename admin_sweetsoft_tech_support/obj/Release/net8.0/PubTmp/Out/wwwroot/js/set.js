pageTipsFun();
function pageTipsFun() {
    var browser = navigator.appName;
    var b_version = navigator.appVersion;
    var version = b_version.split(";");
    if(version[1] === undefined) return;
    var trim_Version = version[1].replace(/[ ]/g, "");
    if (browser == "Microsoft Internet Explorer") {
        if(trim_Version == "MSIE9.0" || trim_Version == "MSIE8.0" || trim_Version == "MSIE7.0" || trim_Version == "MSIE6.0" || trim_Version == "MSIE5.0"){
            document.write('<div class="pageTips" id="pageTips" style="z-index: 2147483647;"><div class="container"><a href="javascript:;" onclick="tipsHide()" class="close"></a><div class="img"><img src="image/nimg273.png" alt="" /></div><div class="msg">抱歉，我们不再支持您的浏览器。请升级您的Internet Explorer（IE）浏览器到最新版本( IE 10以上版本)，或将搜狗、360、QQ 等浏览器切换到急速模式，您还可以下载安装谷歌、火狐浏览器以达到最佳观看模式。</div></div></div>')
        }
    }
}
function tipsHide() {
    document.getElementById('pageTips').style.visibility = "hidden"
}

fontSize();
$(window).resize(function () {
    fontSize();
});

function fontSize() {
    var size;
    var winW = $(window).width();
    if (winW <= 1600 && winW > 800) {
        size = Math.round(winW / 16);
    } else if (winW <= 800) {
        size = Math.round(winW / 7.5);
        if (size > 65) {
            size = 65;
        }
    } else {
        size = 100;
    }
    $('html').css({
        'font-size': size + 'px'
    })
}

$(function () {
    setTimeout(function () {
        $('body').addClass('show');
    }, 1000)
});