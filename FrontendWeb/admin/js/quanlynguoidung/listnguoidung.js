// =========================
// ⚙️ Cấu hình API
// =========================
const userBaseUrl = 'http://localhost:5150/api/NguoiDung';

// =========================
// 🔧 Hàm fetch JSON
// =========================
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    const text = await res.text();
    if (!res.ok) {
        let message = text;
        try {
            const json = JSON.parse(text);
            message = json.message || JSON.stringify(json);
        } catch { }
        throw new Error(message);
    }
    return text ? JSON.parse(text) : {};
}

// =========================
// 🔒 Escape HTML
// =========================
function escapeHtml(str) {
    return str
        ? str.replace(/[&<>"']/g, t => (
            { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[t]
        ))
        : '';
}

// =========================
// 👥 Load danh sách người dùng
// =========================
async function loadUsers() {
    const users = await fetchJson(userBaseUrl);
    const table = $('#usersTable').DataTable();
    table.clear();

    const viTriMap = {
        'admin': 'Admin',
        'nhan vien': 'Nhân viên',
        'khachhang': 'Khách hàng'
    };

    users.forEach(u => {
        table.row.add([
            u.id,
            escapeHtml(u.hoTen),
            escapeHtml(u.tenDn),
            escapeHtml(u.email),
            escapeHtml(u.sdt.replace(/(\d{4})(?=\d)/g, '$1 ')),
            escapeHtml(viTriMap[u.viTri?.toLowerCase()] || u.viTri),

            (() => {
                const status = (u.trangThai || '').toLowerCase();
                let colorClass = 'badge-secondary';
                if (status.includes('đang')) colorClass = 'badge-success';
                else if (status.includes('ngừng')) colorClass = 'badge-danger';
                return `<span class="badge ${colorClass} px-3 py-2">${escapeHtml(u.trangThai)}</span>`;
            })(),

            `
            <button class="btn btn-sm btn-info edit-btn" data-id="${u.id}">
                <i class="fas fa-edit"></i> Sửa
            </button>
            <button class="btn btn-sm btn-warning password-btn" data-id="${u.id}" data-ten-dn="${u.tenDn}">
                <i class="fas fa-key"></i> Đổi mật khẩu
            </button>
            `
        ]);
    });

    table.draw();
}

// =========================
// 🏁 Khởi tạo DataTable
// =========================
function initDataTable() {
    $('#usersTable').DataTable({
        order: [[0, 'desc']],
        columnDefs: [{ orderable: false, targets: -1 }],
        autoWidth: true,
        responsive: true,
        language: {
            url: 'https://cdn.datatables.net/plug-ins/2.0.7/i18n/vi.json'
        }
    });
}

// =========================
// ➕ Mở modal thêm mới
// =========================
function openModalForCreate() {
    $("#modalTitle").text("Thêm người dùng mới");
    $("#userFormAdd")[0].reset();

    $("#Id").val("");
    $("#passwordGroup").show();
    $("#tenDnGroup").show();
    $("#trangThaiGroup").hide();
    $("#ViTri").closest(".form-group").show();

    $("#userModal").modal("show");
}

// =========================
// ✏️ Chỉnh sửa
// =========================
async function editUser(id) {
    const u = await fetchJson(`${userBaseUrl}/${id}`);

    $("#modalTitle").text("Sửa thông tin người dùng");
    $("#userFormEdit")[0].reset();

    $("#Id").val(u.id);
    $("#HoTen").val(u.hoTen);
    $("#Email").val(u.email);
    $("#Sdt").val(u.sdt);
    $("#ViTri").val(u.viTri);
    $("#TrangThai").val(u.trangThai);

    $("#passwordGroup").hide();
    $("#tenDnGroup").hide();
    $("#trangThaiGroup").show();
    $("#ViTri").closest(".form-group").hide();

    $("#userModal").modal("show");
}

// =========================
// 💾 Lưu người dùng
// =========================
async function saveUser() {
    const id = $("#Id").val().trim();

    const hoTen = $("#HoTen").val().trim();
    const tenDn = $("#TenDn").val().trim();
    const matKhau = $("#MatKhau").val().trim();
    const email = $("#Email").val().trim();
    const sdt = $("#Sdt").val().trim();

    // Validation
    if (!hoTen) return showToast("Họ tên không được để trống!", "error");

    if (!id) {
        if (!tenDn || tenDn.length < 4)
            return showToast("Tên đăng nhập phải từ 4 ký tự!", "error");

        if (!matKhau || matKhau.length < 6)
            return showToast("Mật khẩu phải từ 6 ký tự!", "error");
    }

    if (email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email))
            return showToast("Email không hợp lệ!", "error");
    }

    if (sdt) {
        const phoneRegex = /^[0-9]{9,11}$/;
        if (!phoneRegex.test(sdt))
            return showToast("Số điện thoại phải từ 9–11 số!", "error");
    }

    try {
        const list = await fetchJson(userBaseUrl);

        if (!id && list.some(u => u.tenDn.toLowerCase() === tenDn.toLowerCase()))
            return showToast("Tên đăng nhập đã tồn tại!", "error");

        if (list.some(u => u.id != id && u.email === email))
            return showToast("Email đã tồn tại!", "error");

        if (list.some(u => u.id != id && u.sdt === sdt))
            return showToast("SĐT đã tồn tại!", "error");

        const payload = id ? {
            hoTen, email, sdt,
            viTri: $("#ViTri").val(),
            trangThai: $("#TrangThai").val()
        } : {
            hoTen, tenDn, matKhau, email, sdt,
            viTri: $("#ViTri").val()
        };

        const method = id ? 'PUT' : 'POST';
        const url = id ? `${userBaseUrl}/${id}` : userBaseUrl;

        await fetchJson(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        $("#userModal").modal("hide"); // BS4 chuẩn
        showToast(id ? "Cập nhật thành công!" : "Thêm mới thành công!", "success");
        await loadUsers();

    } catch (err) {
        showToast(err.message || "Lỗi lưu người dùng!", "error");
    }
}

// =========================
// 🔐 Đổi mật khẩu
// =========================
async function changePassword() {
    const id = $("#ChangePassId").val();
    const pass1 = $("#NewMatKhau").val().trim();
    const pass2 = $("#NewMatKhauConfirm").val().trim();

    if (pass1.length < 6) return showToast("Mật khẩu phải từ 6 ký tự!", "error");
    if (pass1 !== pass2) return showToast("Mật khẩu xác nhận không khớp!", "error");

    try {
        await fetchJson(`${userBaseUrl}/${id}/doimatkhau`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ matKhauMoi: pass1 })
        });

        $("#changePasswordModal").modal("hide");
        showToast("Đổi mật khẩu thành công!", "success");

    } catch (err) {
        showToast(err.message || "Lỗi đổi mật khẩu!", "error");
    }
}

// =========================
// 🔔 Toast Bootstrap 4
// =========================
function showToast(message, type = "success") {
    const bg = type === "success" ? "bg-success" : "bg-danger";
    const id = "toast" + Date.now();

    const html = `
        <div id="${id}" class="toast ${bg} text-white" role="alert" data-delay="3000">
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    $("#toastContainer").append(html);
    $("#" + id).toast("show").on("hidden.bs.toast", function () {
        $(this).remove();
    });
}

// =========================
// ⚡ Gắn sự kiện
// =========================
document.addEventListener("DOMContentLoaded", async () => {
    initDataTable();
    await loadUsers();

    $("#addUserBtn").click(openModalForCreate);
    $("#saveBtn").click(saveUser);
    $("#saveNewPassBtn").click(changePassword);

    $(document).on("click", ".edit-btn", function () {
        editUser($(this).data("id"));
    });

    $(document).on("click", ".password-btn", function () {
        $("#changePassModalTitle").text("Đổi mật khẩu cho: " + $(this).data("ten-dn"));
        $("#ChangePassId").val($(this).data("id"));
        $("#changePasswordModal").modal("show");
    });
});
