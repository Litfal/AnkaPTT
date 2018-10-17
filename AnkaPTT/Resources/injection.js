var selectedIndex = -1;
var highlightIndices;

function getAllPushObjects() {
    // refreshPushObjects();
    return $.map($('.push'), getPhshObject);
}

function getPhshObject(e, i) {
    var jo = $(e);
    var content = jo.find('.push-content').text().substring(1);
    var ipdatetime = jo.find('.push-ipdatetime').text().trim();
    var ip_d_t = ipdatetime.split(' ');
    var time = (ip_d_t.length >= 1) ? ip_d_t[ip_d_t.length - 1] : "";
    var date = (ip_d_t.length >= 2) ? ip_d_t[ip_d_t.length - 2] : "";
    var ip = (ip_d_t.length >= 3) ? ip_d_t[ip_d_t.length - 3] : "";
    return {
        index: i,
        tag: jo.find('.push-tag').text().trim(),
        userid: jo.find('.push-userid').text().trim(),
        content: content.trim(),
        ipdatetime: jo.find('.push-ipdatetime').text().trim(),
        ip: ip,
        date: date,
        time: time,
    };
}

function scrollToElem(elem) {
    $([document.documentElement, document.body]).animate({
        scrollTop: elem.offsetTop
    }, 500);
}

function scrollToPush(index) {
    scrollToElem($('.push')[index]);
}

function selectPush(index) {
    if (index === selectedIndex) return;
    if (selectedIndex >= 0) {
        $($('.push')[selectedIndex]).css('background-color', '');
    }
    if (index >= 0) {
        var jo = $('.push')[index];
        if (jo) {
            $(jo).css('background-color', '#404040');
            scrollToElem(jo);
        }
    }
    selectedIndex = index;
}

function clickToggleAuto() {
    $('#article-polling').click();
}

function fetch() {
    var errorJo = $('#article-polling');
    return {
        'fatalerror': errorJo.length ? errorJo.hasClass('fatal-error') : null,
        'pushes': getAllPushObjects(),
    };
}

function highlight(indices) {
    var pushesJo = $('.push');
    if (highlightIndices) {
        // unhighlight
        highlightIndices.forEach(function (index) {
            var jo = pushesJo[index];
            if (jo) $(jo).css('background-color', '');
        });
    }
    indices.forEach(function (index) {
        var jo = pushesJo[index];
        if (jo) $(jo).css('background-color', '#004000');
    });
    highlightIndices = indices;
}
