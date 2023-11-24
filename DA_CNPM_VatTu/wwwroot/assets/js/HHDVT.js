
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
            var hh = result.hh;
            var maxTon = result.maxTon;
            var le = result.le;
            var si = result.si;
            var checkCanhBao = (hh.giaBanLe <= (maxTon * le)) || (hh.giaBanSi <= (maxTon * si)) || (hh.giaBanLe == null && hh.giaBanSi == null && hh.tiLeLe == null && hh.tiLeSi == null);
            updateTableGiaDVT(result.hhDvts, result.si, result.le, maxTon);
            $('#tBodyDVT').prepend(`<tr class="${checkCanhBao ? 'text-danger' : ''}" data-id="${hh.id}" style="white-space:nowrap;background-color:aliceblue;">
                        <td>
                            ${hh.iddvtchinhNavigation.tenDvt}(Chính)
                        </td>
                        <td class="text-end">
                            1
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(hh.tiLeLe)}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(hh.tiLeSi)}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(hh.giaBanLe)}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(hh.giaBanSi)}
                        </td>
                        <td class="last-td-column">
                             <div class="action justify-content-end">
                                 <button ${_qSua ? "" : "disabled"} onclick="showModalHHDVTC(${hh.id})" class="text-primary">
                                       <i class="lni lni-pencil"></i>
                                 </button>
                             </div>
                        </td>
                 </tr>`);
            updateTableTonKho(result.hangTonKhos);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
function updateTableGiaDVT(datas, si, le, maxTon) {
    $('#tBodyDVT').empty();
    datas.forEach(function (data) {
        var checkCanhBao = (data.giaBanLe <= (maxTon * le * data.slquyDoi)) || (data.giaBanSi <= (maxTon * si * data.slquyDoi)) || (data.giaBanLe == null && data.giaBanSi == null && data.tiLeLe == null && data.tiLeSi == null);
        $('#tBodyDVT').append(`<tr class="${checkCanhBao ? 'text-danger' : ''}" data-id="${data.id}" style="white-space:nowrap;">
                        <td>
                            ${data.iddvtNavigation.tenDvt}
                        </td>
                        <td class="text-end">
                            ${data.slquyDoi}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(data.tiLeLe) }
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(data.tiLeSi)}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(data.giaBanLe)}
                        </td>
                        <td class="text-end">
                            ${formatOddNumber(data.giaBanSi)}
                        </td>
                        <td class="last-td-column">
                             <div class="action justify-content-end">
                                 <button ${_qSua ? "" : "disabled"} onclick="showModalHHDVT(${data.id})" class="text-primary">
                                       <i class="lni lni-pencil"></i>
                                 </button>
                                <button onclick="deleteDVTHH(${data.id})" ${_qXoa ? "" : "disabled"} class="text-danger">
                                    <i class="lni lni-trash-can"></i>
                                </button>
                             </div>
                        </td>
                 </tr>`);
    });
}
function updateTableTonKho(datas) {
    $('#tBodyGia').empty();
    datas.forEach(function (ht) {
        $('#tBodyGia').append(`<tr style="white-space:nowrap;">
                                                    <td>
                            ${ht.idhhNavigation.iddvtchinhNavigation.tenDvt} (Chính)
                                                    </td>
                                                    <td class="text-center">
                            ${formatDay(ht.ngayNhap)}
                                                    </td>
                                                    <td class="text-center">
                            ${formatDay(ht.hsd)}
                                                    </td>
                                                    <td class="text-end">
                            ${formatOddNumber(ht.slcon)}
                                                    </td>
                                                    <td class="text-end">
                            ${formatOddNumber(ht.giaNhap * (1 - ht.cktm/100) * (1 + ht.thue/100))}
                                                    </td>
                                            </tr>`);
    });
}
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
/*
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
});*/
$('#search').on('keyup', function (e) {
    var key = $(this).val().toLowerCase();
    $('#tBodyDVT').empty();
    $('#tBodyGia').empty();
    var listItem = $('#tBodyHangHoa tr');

    listItem.filter(function () {
        var have = $(this).text().toLowerCase().indexOf(key) > -1;
        $(this).toggle(have);
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
$('#btnModal').on('click', function (){
    /*if ($('#form').data('val') === "chinh") {
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
    }*/
    var form = document.getElementById('formUpdate');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        document.getElementById('DVT').selectize.$control[0].classList.add('is-invalid');
    } else {
        form.classList.remove('was-validated');
        document.getElementById('DVT').selectize.$control[0].classList.remove('is-invalid');

        if ($('#formUpdate').data('val') === "chinh") {
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
        } else {
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