// 자산 등록 화면: 분류 변경 시 다음 자산번호 자동 제안
document.addEventListener('DOMContentLoaded', function () {
    var cat = document.getElementById('CategoryId');
    var tag = document.getElementById('AssetTag');
    var btn = document.getElementById('btn-next-tag');
    if (!cat || !tag || !btn) return;

    function fetchTag() {
        if (!cat.value || cat.value === '0') return;
        fetch(btn.dataset.url + '?categoryId=' + encodeURIComponent(cat.value), { credentials: 'same-origin' })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (j) { if (j && j.tag) tag.value = j.tag; });
    }
    btn.addEventListener('click', fetchTag);
    cat.addEventListener('change', function () { if (btn.dataset.auto === 'true') fetchTag(); });
});

// 폼 제출 중복 방지
document.querySelectorAll('form[data-once]').forEach(function (f) {
    f.addEventListener('submit', function (e) {
        var b = f.querySelector('button[type=submit]');
        if (b) { b.disabled = true; b.textContent = document.body.dataset.busyText || b.textContent; }
    });
});
