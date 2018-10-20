var selectedPushElem = null;

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
    if (selectedPushElem) {
        $(selectedPushElem).removeClass('push-sel');
        selectedPushElem = null;
    }
    if (index >= 0) {
        var jo = $('.push')[index];
        if (jo) {
            $(jo).addClass('push-sel');
            scrollToElem(jo);
            selectedPushElem = jo;
        }
    }
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

function highlight(key,indices) {
    var pushesJo = $('.push');
    // unhighlight
    pushesJo.removeClass('push-hl' + key);
    indices.forEach(function (index) {
        var jo = pushesJo[index];
        if (jo) $(jo).addClass('push-hl' + key);
    });
}

function updateMainContent(encodedHtml) {
    $('#main-container').html(unescape(encodedHtml));
}

function addPushes(pushes) {
    for (var i = 0; i < pushes.length; i++) {
        $('#main-content').append(unescape(pushes[i]));
    }
}

// add css for push
$('head').append('<style type="text/css">.push-hl0{background-color:#004000;}.push-hl1{background-color:#400000;}.push-hl2{background-color:#000060;}.push-hl3{background-color:#004040;}.push-hl4{background-color:#300030;}.push-hl5{background-color:#404000;}.push-sel{background-color:#404040;}</style>')
