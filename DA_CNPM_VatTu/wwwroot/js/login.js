
    if (!localStorage.getItem('user')) {
        $('#divPhanCong').hide();
    } else {
        $('#divPhanCong').show();
        $('#divLogin').hide();
        const user = JSON.parse(localStorage.getItem('user'))
        xacThuc(JSON.stringify(user))
    }

$('#loginForm').on('submit', function (e) {
    var u = $('#userName').val();
    var p = $('#passWord').val();
    const user = JSON.stringify({
        UserName: u,
        PassWord: p
    })
    xacThuc(user);
    e.preventDefault();
});

$('#btnHuy').on('click', function () {
    $('#divPhanCong').hide();
    $('#divLogin').show();
    $('#roles').empty();
    $('#chiNhanh option:not(:first)').remove();
    localStorage.removeItem('user');
});

$('#formPhanCong').on('submit', function (e) {
    const user = JSON.parse(localStorage.getItem('user'))
    var cn = $('#chiNhanh').val();
    var r = $('#roles').val();

    fetch("/api/login", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            UserName: user.UserName,
            PassWord: user.PassWord,
            IdChiNhanh: cn,
            IdVt: r
        })
    })
        .then(Response => Response.json())
        .then(data => {
            if (data.statusCode === 200) {
                localStorage.removeItem('user');
                window.location.href = "/TrangChu";
            }
        })
        .catch(error => console.error(error))
    e.preventDefault();
});

function xacThuc(user) {
    fetch("/api/xac-thuc", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: user
    })
        .then(Response => Response.json())
        .then(data => {
            $('#message').remove();
            if (data.statusCode === 200) {
                localStorage.setItem('user', user);
                $('#divPhanCong').show();
                $('#divLogin').hide();
                $('#divMessage').empty();
                data.chiNhanh.map(cn => $('#chiNhanh').append(`<option value=${cn.id}>${cn.tenCn}</option>`))
            } else {
                $('#divMessage').empty();
                $('#divMessage').prepend(`<p style="margin-bottom: 30px;" class='text-sm text-medium text-center text-danger'>${data.message}</p>`);
            }
        })
        .catch(error => console.error(error))
}
function changeChinhNhanh() {
    $.ajax({
        type: "post",
        url: "/api/change-chinhanh",
        data: "Cn=" + $('#chiNhanh').val(),
        success: function (result) {
            $('#roles').empty();
            result.vaiTro.map(cn => $('#roles').append(`<option value=${cn.id}>${cn.tenVt}</option>`))
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}