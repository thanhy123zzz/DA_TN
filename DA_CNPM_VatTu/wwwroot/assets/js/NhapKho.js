var _idPn = 0;
var _daXoa = [];
var _active = true;
$(document).ready(function () {
    var datas = [
        {
            el: $('.cbNhaCC'),
            placeholder: '-- Nhà cung cấp --',
            url: '/QuanLy/NhapKho/api/nccs'
        }
    ];
    configCb(datas);
    configDate();
    configDateTime();
    formatNumberFloatWithElement($('.input-number-float'));

    loadTable();

    $(document).on('click', '.btn-remove-ct', function () {
        var tr = $(this).closest('tr');
        var id = tr.find('input[name="Id"]').val();
        if (id) {
            _daXoa.push(parseInt(id));
        }
        if (!tr.is(":last-child")) {
            tr.find('select').each(function () {
                if (this.selectize) {
                    this.selectize.destroy();
                }
            });
            tr.remove();
            updateGiaTongTien();
        }
    });
    $(document).on('keyup', 'input.SlDvt', function () {
        var tr = $(this).closest('tr');
        var soLuongNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var donGiaNhap = parseFloat(tr.find('input.DonGiaDvt').inputmask('unmaskedvalue'));

        // thay đổi thành tiền
        if (donGiaNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();

            var slqd = parseFloat(tr.find('input.slqd').inputmask('unmaskedvalue'));
            tr.find('input[name="Sl"]').val(slqd * soLuongNhap);
            tr.find('input[name="DonGia"]').val(donGiaNhap / slqd);
        }
    });
    $(document).on('keyup', 'input.DonGiaDvt', function () {
        var tr = $(this).closest('tr');
        var donGiaNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var soLuongNhap = parseFloat(tr.find('input.SlDvt').inputmask('unmaskedvalue'));

        if (soLuongNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();

            var slqd = parseFloat(tr.find('input.slqd').inputmask('unmaskedvalue'));
            tr.find('input[name="Sl"]').val(slqd * soLuongNhap);
            tr.find('input[name="DonGia"]').val(donGiaNhap / slqd);
        }
    });

    // thay đổi số lượng nhập và đơn giá nếu thành tiền thay đổi
    $(document).on('keyup', 'input.ThanhTien', function () {
        var tr = $(this).closest('tr');
        var thanhTien = $(this).inputmask('unmaskedvalue');
        var soLuongNhap = tr.find('input.SlDvt').inputmask('unmaskedvalue');
        var donGiaNhap = tr.find('input.DonGiaDvt').inputmask('unmaskedvalue');

        // thay đổi thành tiền
        if (donGiaNhap !== "" || soLuongNhap !== "") {
            if (soLuongNhap !== "") {
                tr.find('input.DonGiaDvt').val(thanhTien / soLuongNhap);
                updateGiaTongTien();
                return;
            }
            if (donGiaNhap !== "") {
                tr.find('input.SlDvt').val(thanhTien / donGiaNhap);
                updateGiaTongTien();
                return;
            }
            var slqd = parseFloat(tr.find('input.slqd').inputmask('unmaskedvalue'));
            tr.find('input[name="Sl"]').val(slqd * soLuongNhap);
            tr.find('input[name="DonGia"]').val(donGiaNhap / slqd);
        }
    });
    $(document).on('keyup', 'input[name="Cktm"], input[name="Thue"]', function () {
        updateGiaTongTien();
    });

    $('#btnTaoPhieu').on('click', function () {
        var form = document.getElementById('formTaoPhieuNhap');
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
        } else {
            if (!$('#nhaCC').val() || !$('#NgayNhap').val()) {
                if (!$('#nhaCC').val()) {
                    showToast("Chưa chọn nhà cung cấp!", 500);
                }
                else {
                    showToast("Chưa nhập ngày tạo!", 500);
                }
                return;
            }
            spinnerBtn($('#btnTaoPhieu'));
            // Lắng nghe sự kiện click trên nút có class "action-button"
            var phieuNhapCts = [];
            var check = true;
            $("#tBodyCtpn tr[data-daxuat='false']").each(function () {
                var tr = $(this);
                var idHh = tr.find('select[name="Idhh"]').val();
                if (idHh) {
                    tr.removeClass('bg-danger');
                    var slNhap = tr.find('input[name="Sl"]').inputmask('unmaskedvalue');
                    /*var chiecKhau = tr.find('input[name="Cktm"]').inputmask('unmaskedvalue');
                    var thueVat = tr.find('input[name="Thue"]').inputmask('unmaskedvalue');*/
                    if (slNhap) {
                        tr.find('input[name="Sl"]').closest('td').removeClass('bg-danger');

                        var data = getDataFromTr(tr);
                        phieuNhapCts.push(data);
                    } else {
                        if (!slNhap) {
                            tr.find('input[name="Sl"]').closest('td').addClass('bg-danger');
                        }
                        check = false;
                        showBtn($('#btnTaoPhieu'), 'Lưu');
                    }
                } else {
                    if (!tr.is(":last-child")) {
                        tr.addClass('bg-danger');
                    }
                }
            });
            if (phieuNhapCts.length == 0 && _idPn == 0) {
                check = false;
            }
            if (check) {
                var phieuNhap = {
                    Id: _idPn,
                    Idncc: $('#nhaCC').val(),
                    SoHd: $('#soHD').val(),
                    NgayTao: $('#NgayNhap').val(),
                    NgayHd: $('#ngayHD').val(),
                    GhiChu: $('#ghiChu').val(),
                    ChiTietPhieuNhaps: phieuNhapCts,
                    DaXoas: _daXoa
                }
                console.log(phieuNhap);
                $.ajax({
                    type: "post",
                    url: "/QuanLy/NhapKho/update-pn",
                    data: JSON.stringify(phieuNhap),
                    contentType: "application/json",
                    success: function (response) {
                        showToast(response.message, response.statusCode);
                        showBtn($('#btnTaoPhieu'), 'Lưu');
                        if (response.statusCode == 200) {
                            $('#btnLoadLsNhap').click();
                            if (!_idPn) {
                                xoaTrangPhieuXuatKho();
                            } else {
                                showEditPhieuNhap(response.result);
                            }
                        }
                    },
                    error: function (error) {
                        console.log(error);
                        showBtn($('#btnTaoPhieu'), 'Lưu');
                    }
                });
            } else {
                showBtn($('#btnTaoPhieu'), 'Lưu');
            }
        }
    });
    $('#btnXoaTrang').on('click', function () {
        xoaTrangPhieuXuatKho();
    });
    $('#btnLoadLsNhap').on('click', function () {
        showLoader($('#LichSuNhap'));

        var toDay = $('#toDay').val();
        var fromDay = $('#fromDay').val();
        var nhacc = $('#nhaCCLS').val();
        var hhLS = $('#hangHoaLS').val();
        var soPhieuLS = $("#soPhieuLS").val();
        var soHDLS = $('#soHDLS').val();
        $.ajax({
            type: "post",
            url: "/QuanLy/NhapKho/loadTableLichSuNhap",
            data: "toDay=" + toDay + "&fromDay=" + fromDay + "&nhaCC="
                + nhacc + "&hhLS=" + hhLS + "&soPhieuLS=" + soPhieuLS + "&soHDLS=" + soHDLS,
            success: function (result) {
                hideLoader();
                $('#tBodyLSNhap').empty();
                $('#tBodyLSNhap').append(result);
            },
            error: function () {
                alert("Fail");
            }
        });
    });

    $(document).on('click', '.btn-view-pn', function () {
        $('#bordered-justified-profile').removeClass('active');
        $('#bordered-justified-profile').removeClass('show');
        $('#tabXemPhieu').removeClass('d-lg-none');
        var id = $(this).val();
        $.ajax({
            type: "post",
            url: "/QuanLy/NhapKho/ViewThongTinPhieuNhap",
            data: "idPN=" + id,
            success: function (result) {
                $('#tabXemPhieu').replaceWith(result);
            },
            error: function () {
                alert("Fail");
            }
        });
    });
    $(document).on('change', 'select.dvt', function () {

        var tr = $(this).closest('tr');
        var slqd = $(this).find('option:selected').data('slqd');

        tr.find('input.slqd').val(slqd);
        tr.find('input.SlDvt').keyup();
    });

    $(document).on('click', '.btn-remove-pn', function () {
        if (confirm("Bạn có muốn thực hiện thao tác này?")) {
            var id = $(this).val();
            var tr = $(this).closest('tr');
            if (id) {
                $.ajax({
                    type: "post",
                    url: "/QuanLy/NhapKho/removePhieuNhap",
                    data: "idPN=" + id,
                    success: function (result) {
                        showToast(result.message, result.statusCode);
                        if (result.statusCode == 200) {
                            tr.remove();
                        }
                    },
                    error: function () {
                        alert("Fail");
                    }
                });
            }
        }
    });
    $(document).on('click', '.btn-edit-pn', function () {
        var id = $(this).val();
        if (id) {
            _active = false;
            $.ajax({
                type: "post",
                url: "/QuanLy/NhapKho/showEditPhieuNhap",
                data: "idPN=" + id,
                success: function (result) {
                    showEditPhieuNhap(result);
                },
                error: function () {
                    alert("Fail");
                }
            });
        }
    });
});
function loadTable() {
    $.ajax({
        type: 'POST',
        url: '/QuanLy/NhapKho/api/hhs',
        data: "active=" + _active
    }).done(function (response) {
        $('#tBodyCtpn').append(getRowPhieuNhapCt());
        configRowPhieuNhapCt($('#tBodyCtpn tr:last'), response);

        $('#hangHoaLS').selectize({
            maxOptions: 50,
            valueField: "id",
            labelField: "ten",
            searchField: ["ten", "ma"],
            placeholder: "-- Hàng hoá --",
            loadThrottle: 400,
            options: response,
            render: {
                option: function (item, escape) {
                    return `<div class="px-2 py-1"><b>[${item.ma}]</b> - ${item.ten}</div>`;
                },
                no_results: function (data, escape) {
                    return '<div class="no-results">Không tìm thấy dữ liệu </div>';
                },
            },
        });
    });
}
function xoaTrangPhieuXuatKho() {
    _idPn = 0;
    _daXoa = [];
    _active = true;
    $('#TienHang').val(0);
    $('#TienCK').val(0);
    $('#TienThue').val(0);
    $('#TienThanhToan').val(0);
    $.ajax({
        type: "post",
        url: "/QuanLy/NhapKho/api/getSoPhieuNhap",
        success: function (result) {
            $('#maPhieu').val(result);
        },
        error: function () {
            alert("Fail");
        }
    });
    $('#nhaCC')[0].selectize.clear();
    $('#NgayNhap').val(getDateTimeNow());
    $('#ngayHD').val(getDateNow());
    $('#soHD').val('');
    $('#ghiChu').val('');

    $('#tBodyCtpn tr').each(function () {
        var tr = $(this);

        tr.find('select').each(function () {
            if (this.selectize) {
                this.selectize.destroy();
            }
        });
    });
    $('#tBodyCtpn').empty();
    loadTable();
}
function getRowPhieuNhapCt() {
    return `<tr data-daxuat="false">
        <td><select class="form-select form-table" name="Idhh" style="width: 300px;"></select>
        <input type="hidden" value="0" name="Id"/>
        </td>
        <td>
            <select class="select-control form-table dvt" style="width: 80px; height: 30px" name="Iddvtnhap">
                
            </select>
        </td>
        <td><input autocomplete="off" class="form-control form-table input-number-float slqd" name="Slqd" style="min-width: 60px;" readonly/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float SlDvt" style="min-width: 80px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float DonGiaDvt" style="min-width: 120px;"/></td>
        <td><input autocomplete="off" class="form-control form-table ThanhTien input-number-float" style="min-width: 140px;"/></td>
        <td><select class="form-select form-table" name="SoLo" style="width: 120px;"></select></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Cktm" max="100" style="min-width: 60px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Thue" max="100" style="min-width: 60px;"/></td>
        <td><input autocomplete="off" class="form-control form-table date-sort-mask" name="Hsd" style="min-width: 110px;"/></td>
        <td><input autocomplete="off" class="form-control form-table date-sort-mask" name="Nsx" style="min-width: 110px;"/></td>
        <td><textarea autocomplete="off" class="form-control form-table" name="GhiChu" style="min-width: 220px;" rows="1"></textarea></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Sl" style="min-width: 80px;" readonly/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="DonGia" style="min-width: 120px;" readonly/></td>
        <td class='last-td-column'>
            <div class="action justify-content-center">
                <button class="text-danger btn-remove-ct">
                    <i class="lni lni-trash-can"></i>
                </button>
            </div>
        </td>
    </tr>`;
}
function configRowPhieuNhapCt(tr, hhs) {
    var cbHangHoa = tr.find('select[name="Idhh"]');
    cbHangHoa.selectize({
        maxOptions: 50,
        valueField: "id",
        labelField: "ten",
        searchField: ["ten", "ma"],
        placeholder: '-- Hàng hoá --',
        options: hhs,
        loadThrottle: 400,
        dropdownParent: '#dropdow-show',
        closeAfterSelect: 0,
        onDropdownOpen: function ($dropdown) {
            showDropdownMenu(cbHangHoa, $dropdown);
        },
        onChange: function (value) {
            dropDownHhChange(cbHangHoa, value);
        },
        onFocus: function ($dropdown) {
            $('.my-selectize-2').not(this.$input).each(function () {
                if (this.selectize) {
                    this.selectize.close();
                    this.selectize.blur();
                }
            });
        },
        render: {
            option: function (item, escape) {
                return `<div class="px-2 py-1"><b>[${item.ma}]</b> - ${item.ten}</div>`;
            },
            no_results: function (data, escape) {
                return '<div class="no-results">Không tìm thấy dữ liệu </div>';
            },
        },
    });
    formatNumberFloatWithElement($('tr .input-number-float'));
    configDateLongMask($('.date-sort-mask'));
}
function dropDownHhChange(cbHangHoa, value) {
    var tr = cbHangHoa.closest('tr');
    var selectize = cbHangHoa[0].selectize;
    if (selectize && value) {
        var options = Object.values(selectize.options);
        var option = options.find(function (item) {
            return item.id == value;
        });
        var soLos = [];
        option.soLos.forEach(function (item) {
            soLos.push({
                soLo: item
            });
        });
        var cbDvt = tr.find('select.dvt');
        cbDvt.empty();
        var dvts = ``;
        //option.dvts.unshift(option.dvtChinh);
        dvts += `<option value="${option.dvtChinh.id}" data-slqd="${option.dvtChinh.slqd}">${option.dvtChinh.ten}</option>`
        option.dvts.forEach(function (item) {
            dvts += `<option value="${item.id}" data-slqd="${item.slqd}">${item.ten}</option>`
        })
        cbDvt.append(dvts);
        tr.find('input.slqd').val(1);

        var cbSoLo = tr.find('select[name="SoLo"]');

        if (cbSoLo[0].selectize) {
            cbSoLo[0].selectize.clear();
            cbSoLo[0].selectize.clearOptions();
            cbSoLo[0].selectize.addOption(soLos);
        } else {
            cbSoLo.selectize({
                placeholder: "-- Số lô --",
                options: soLos,
                valueField: "soLo",
                labelField: "soLo",
                searchField: ["soLo"],
                create: true,
                dropdownParent: '#dropdow-show',
                closeAfterSelect: 0,
                onDropdownOpen: function ($dropdown) {
                    showDropdownMenu(cbSoLo, $dropdown);
                },
                onFocus: function ($dropdown) {
                    $('.my-selectize-2').not(this.$input).each(function () {
                        if (this.selectize) {
                            this.selectize.close();
                            this.selectize.blur();
                        }
                    });
                },
            });
        }
        if (tr.is(':last-child')) {
            $('#tBodyCtpn').append(getRowPhieuNhapCt());
            configRowPhieuNhapCt($('#tBodyCtpn tr:last'), options);
        }
    }
}
function updateGiaTongTien() {

    var tongTienHang = 0;
    var tongTienCK = 0;
    var tongTienThue = 0;
    var tongTra = 0;
    $('#tBodyCtpn tr').each(function () {
        var tr = $(this);

        var SlxGia = parseFloat(tr.find('input.ThanhTien').inputmask('unmaskedvalue'));

        if (SlxGia) {
            tongTienHang += parseFloat(SlxGia);
        }

        var chiecKhau = parseFloat(tr.find('input[name="Cktm"]').inputmask('unmaskedvalue'));
        var tienChiecKhau = 0;
        if (chiecKhau && SlxGia) {
            tienChiecKhau = ((chiecKhau * SlxGia) / 100);
            tongTienCK += tienChiecKhau;
        }

        var thueVat = parseFloat(tr.find('input[name="Thue"]').inputmask('unmaskedvalue'));
        var tienThue = 0;
        if (thueVat && SlxGia) {
            tienThue = (((SlxGia - tienChiecKhau) * thueVat) / 100);
            tongTienThue += tienThue;
        }
    })
    $('#TienHang').val(tongTienHang);
    $('#TienCK').val(tongTienCK);
    $('#TienThue').val(tongTienThue);
    $('#TienThanhToan').val(tongTienHang - tongTienCK + tongTienThue);
}
function cancelXemPhieu() {
    $('#tabXemPhieu').addClass('d-lg-none');
    $('#bordered-justified-profile').addClass('active');
    $('#bordered-justified-profile').addClass('show');
}

function showEditPhieuNhap(result) {
    console.log(result)
    _idPn = result.id;
    _daXoa = [];
    $('#maPhieu').val(result.soPn);
    $('#nhaCC')[0].selectize.setValue(result.idncc);
    $('#NgayNhap').val(formatDateTime(result.ngayTao));
    $('#soHD').val(toEmpty(result.soHd));
    $('#ngayHD').val(formatDay(result.ngayHd));
    $('#ghiChu').val(toEmpty(result.ghiChu));

    $('#tBodyCtpn tr').each(function () {
        var tr = $(this);

        tr.find('select').each(function () {
            if (this.selectize) {
                this.selectize.destroy();
            }
        });
    });
    $('#tBodyCtpn').empty();

    $.ajax({
        type: 'POST',
        url: '/QuanLy/NhapKho/api/hhs',
        data: "active=" + _active
    }).done(function (response) {
        result.chiTietPhieuNhaps.forEach(function (item, index) {
            var daXuat = item.hangTonKhos[0].slcon != item.sl;
            var hh = response.find(function (data) {
                return data.id == item.idhh;
            });
            var dvts = ``;
            dvts += `<option value="${hh.dvtChinh.id}" data-slqd="${hh.dvtChinh.slqd}">${hh.dvtChinh.ten}</option>`
            //hh.dvts.unshift(hh.dvtChinh);
            hh.dvts.forEach(function (data) {
                dvts += `<option ${(data.id == item.iddvtnhap ? "selected" : "")} value="${data.id}" data-slqd="${data.slqd}">${data.ten}</option>`
            });
            var soLos = [];
            hh.soLos.forEach(function (item) {
                soLos.push({
                    soLo: item
                });
            });

            $('#tBodyCtpn').append(getRowPhieuNhapCtCoDuLieu(item, dvts, daXuat));
            var tr = $('#tBodyCtpn tr:last');
            var cbSoLo = tr.find('select[name="SoLo"]');
            cbSoLo.selectize({
                placeholder: "-- Số lô --",
                options: soLos,
                valueField: "soLo",
                labelField: "soLo",
                searchField: ["soLo"],
                create: true,
                dropdownParent: '#dropdow-show',
                closeAfterSelect: 0,
                items: [item.soLo],
                onDropdownOpen: function ($dropdown) {
                    showDropdownMenu(cbSoLo, $dropdown);
                },
                onFocus: function ($dropdown) {
                    $('.my-selectize-2').not(this.$input).each(function () {
                        if (this.selectize) {
                            this.selectize.close();
                            this.selectize.blur();
                        }
                    });
                },
            });
            configRowPhieuNhapCt(tr, response);
            if (index == result.chiTietPhieuNhaps.length - 1) {
                tr.find('.SlDvt').keyup();
            }
            if (daXuat) {
                tr.find('select').each(function () {
                    if (this.selectize) {
                        this.selectize.disable();
                    } else {
                        this.disabled = true;
                    }
                });
                tr.find('input').prop('readonly', true);
                tr.find('textarea').prop('readonly', true);
                tr.find('button').prop('disabled', true);
            }
        })
        loadTable();
    });

    var tabLapPhieu = document.getElementById('home-tab');
    var tab = new bootstrap.Tab(tabLapPhieu);
    tab.show();
}
function getRowPhieuNhapCtCoDuLieu(data, dvts, daXuat) {
    return `<tr data-daxuat="${daXuat}">
        <td>
        <select class="form-select form-table" name="Idhh" style="width: 300px;">
            <option value="${data.idhh}"></option>
        </select>
        <input type="hidden" value="${data.id}" name="Id"/>
        </td>
        <td>
            <select class="select-control form-table dvt" name="Iddvtnhap" style="width: 80px; height: 30px">
                ${dvts}
            </select>
        </td>
        <td><input value="${data.slqd}" autocomplete="off" class="form-control form-table input-number-float slqd" name="Slqd" style="min-width: 60px;" readonly/></td>
        <td><input value="${data.sl / data.slqd}" autocomplete="off" class="form-control form-table input-number-float SlDvt" style="min-width: 80px;"/></td>
        <td><input value="${data.donGia * data.slqd}" autocomplete="off" class="form-control form-table input-number-float DonGiaDvt" style="min-width: 120px;"/></td>
        <td><input value="${data.sl * data.donGia}" autocomplete="off" class="form-control form-table ThanhTien input-number-float" style="min-width: 140px;"/></td>
        <td>
        <select class="form-select form-table" name="SoLo" style="width: 120px;">
        </select></td>
        <td><input value="${data.cktm}" autocomplete="off" class="form-control form-table input-number-float" name="Cktm" max="100" style="min-width: 60px;"/></td>
        <td><input value="${data.thue}" autocomplete="off" class="form-control form-table input-number-float" name="Thue" max="100" style="min-width: 60px;"/></td>
        <td><input value="${formatDay(data.hsd)}" autocomplete="off" class="form-control form-table date-sort-mask" name="Hsd" style="min-width: 110px;"/></td>
        <td><input value="${formatDay(data.nsx)}" autocomplete="off" class="form-control form-table date-sort-mask" name="Nsx" style="min-width: 110px;"/></td>
        <td><textarea autocomplete="off" class="form-control form-table" name="GhiChu" style="min-width: 220px;" rows="1">${data.ghiChu}</textarea></td>
        <td><input value="${data.sl}" autocomplete="off" class="form-control form-table input-number-float" name="Sl" style="min-width: 80px;" readonly/></td>
        <td><input value="${data.donGia}" autocomplete="off" class="form-control form-table input-number-float" name="DonGia" style="min-width: 120px;" readonly/></td>
        <td class='last-td-column'>
            <div class="action justify-content-center">
                <button class="text-danger btn-remove-ct">
                    <i class="lni lni-trash-can"></i>
                </button>
            </div>
        </td>
    </tr>`;
}
function offTabNhap() {
    $('#tabXemPhieu').addClass('d-none');
}