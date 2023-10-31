
var idModel = idModel;
var hh = 0;
$(document).on('click', '#tBodyHangHoa tr', function () {
    // loại bỏ lớp active từ tất cả các hàng
    $('#tBodyHangHoa tr').removeClass('table-active');
    // thêm lớp active cho hàng được nhấp vào
    $(this).addClass('table-active');

    var id = this.getAttribute('data-id')
    hh = id;
    $.ajax({
        type: "post",
        url: "/QuyDinh/HHDVT/load-hhdvt",
        data: "idHh=" + id,
        success: function (result) {
            $('#tabHHDVT').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});

function showHHGiaLo() {
    $.ajax({
        type: "get",
        url: "/QuyDinh/HHDVT/change-tt",
        data: "tt=" + document.getElementById('toggleSwitch1').checked,
        success: function (result) {
            $('#tBodyHangHoa').empty();
            $('#tBodyDVT').empty();
            $('#tBodyGia').empty();
            result.data.map(item => $('#tBodyHangHoa').append(`<tr data-id="${item.id}" style="cursor:pointer"><td class="first-td-column">${item.maHh}</td><td>${item.tenHh}</td></tr>`));

        },
        error: function (loi) {
            console.log(loi);
        }
    });
}

$('#search').on('keyup', (e) => {
    var key = e.target.value;
    $('#tBodyDVT').empty();
    $('#tBodyGia').empty();
    $.ajax({
        type: "post",
        url: "/QuyDinh/HHDVT/search-hh",
        data: "key=" + key + "&tt=" + document.getElementById('toggleSwitch1').checked,
        success: function (result) {
            $('#tBodyHangHoa').empty();
            result.data.map(item => $('#tBodyHangHoa').append(`<tr data-id="${item.id}" style="cursor:pointer"><td class="first-td-column">${item.maHh}</td><td>${item.tenHh}</td></tr>`));

        },
        error: function (loi) {
            console.log(loi);
        }
    });
});

$(document).on('focus', '.my-selectize #Slquydoi', function () {
    $('.my-selectize').each(function () {
        $(this).removeClass('is-invalid');
    });
    $('#Slquydoi').removeClass('is-invalid');
});
function showModalHHDVT(idHhdvt) {
    if ($('.table-active').data('id') !== undefined) {
        $('#staticBackdrop').modal('show');
        idModel = idHhdvt;
        $.ajax({
            type: "post",
            url: "/QuyDinh/HHDVT/show-modal-hhdvt",
            data: "idHhdvt= " + idHhdvt + "&idHh=" + hh,
            success: function (result) {
                $('#contentModal').empty();
                $('#contentModal').append(result.view);
                $('#staticBackdropLabel').text(result.title);
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
}
function showModalHHDVTC(idHh) {
        $('#staticBackdrop').modal('show');
        $.ajax({
            type: "post",
            url: "/QuyDinh/HHDVT/show-modal-hhdvtc",
            data: "idHh=" + hh,
            success: function (result) {
                $('#contentModal').empty();
                $('#contentModal').append(result.view);
                $('#staticBackdropLabel').text(result.title);
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    
    
}
function getValue(str) {
    return Number(str.replace(/[^0-9.-]+/g, ""));
}
function checkNumber(str) {
    return /[0-9,.\-$]+/.test(str);
}
$('#btnModal').on('click', (e) => {
    if ($('#form').data('val') === "chinh") {
        var invalid = false;
        $('.my-selectize').each(function () {
            if (this.selectize) {
                if ($(this).val() === "") {
                    $(this).nextAll().addClass("is-invalid");
                    invalid = true;
                }
            }
        });
        if (!checkNumber($('#Slquydoi').val())) {
            $('#Slquydoi').addClass("is-invalid")
            invalid = true;
        }
        if ($('#TLLe').val() === "" && $('#TLSi').val() === "" && $('#GBLe').val() === "" && $('#GBSi').val() === "") {
            $('.text-decimal').addClass("is-invalid");
            invalid = true;
        }
        if (invalid) {
            return false;
        }

        fetch("/QuyDinh/HHDVT/update-hhdvtc", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Id: $('.table-active').attr('data-id'),
                Iddvtchinh: $('#DVT').val(),
                TiLeLe: getValue($('#TLLe').val()),
                TiLeSi: getValue($('#TLSi').val()),
                GiaBanLe: getValue($('#GBLe').val()),
                GiaBanSi: getValue($('#GBSi').val()),
                Active: document.getElementById('toggleSwitch1').checked
            })
        })
            .then(Response => Response.json())
            .then(data => {
                $('#staticBackdrop').modal('hide');
                if (data.statusCode == 200) {
                    $('#tabHHDVT').replaceWith(data.viewData);
                    if (data.data !== undefined) {
                        $('#tBodyHangHoa').empty();
                        data.data.map(item => $('#tBodyHangHoa').append(`<tr data-id="${item.id}" class="${(item.id == hh) ? "table-active" : ""}" style="cursor:pointer"><td>${item.maHh}</td><td>${item.tenHh}</td></tr>`));
                    }
                }
                $('#toast').addClass(data.color);
                $('#toastContent').text(data.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(data.color);
                }, 5000);
            })
            .catch(error => console.error(error))
    }
    else {
        var invalid = false;
        $('.my-selectize').each(function () {
            if (this.selectize) {
                if ($(this).val() === "") {
                    $(this).nextAll().addClass("is-invalid");
                    invalid = true;
                }
            }
        });
        if (!checkNumber($('#Slquydoi').val())) {
            $('#Slquydoi').addClass("is-invalid")
            invalid = true;
        }
        if (invalid) {
            return;
        }

        fetch("/QuyDinh/HHDVT/update-hhdvt", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Id: idModel,
                IdHh: $('.table-active').attr('data-id'),
                IdDvt: $('#DVT').val(),
                TiLeLe: getValue($('#TLLe').val()),
                TiLeSi: getValue($('#TLSi').val()),
                GiaBanLe: getValue($('#GBLe').val()),
                GiaBanSi: getValue($('#GBSi').val()),
                SlquyDoi: getValue($('#Slquydoi').val()),
                Active: document.getElementById('toggleSwitch1').checked
            })
        })
            .then(Response => Response.json())
            .then(data => {
                $('#staticBackdrop').modal('hide');
                if (data.statusCode == 200) {
                    $('#tabHHDVT').replaceWith(data.viewData);
                    if (data.data !== undefined) {
                        $('#tBodyHangHoa').empty();
                        data.data.map(item => $('#tBodyHangHoa').append(`<tr data-id="${item.id}" class="${(item.id == hh) ? "table-active" : ""}" style="cursor:pointer"><td>${item.maHh}</td><td>${item.tenHh}</td></tr>`));
                    }
                }
                $('#toast').addClass(data.color);
                $('#toastContent').text(data.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(data.color);
                }, 5000);
            })
            .catch(error => console.error(error))
    }
});

function deleteDVTHH(id) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $('#tBodyDVT').empty();
        $.ajax({
            type: "post",
            url: "/QuyDinh/HHDVT/remove",
            data: "idHhdvt=" + id + "&idHh=" + idModel,
            success: function (result) {
                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                $('#tBodyDVT').replaceWith(result.viewData);

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(data.color);
                }, 5000);
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
}