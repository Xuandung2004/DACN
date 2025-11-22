// =========================
// ⚙️ Cấu hình API
// =========================
// SỬ DỤNG API CỦA TaiKhoanController
const taiKhoanBaseUrl = 'http://localhost:5150/api/TaiKhoan'; 

// =========================
// 🔧 Hàm tiện ích (Giả định copy từ file Admin của bạn)
// =========================
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    const text = await res.text();
    if (!res.ok) {
        let message = text;
        try {
            const json = JSON.parse(text);
            message = json.message || JSON.stringify(json);
        } catch {}
        throw new Error(message);
    }
    return text ? JSON.parse(text) : {};
}

// Thay thế hàm showToast giả định bằng hàm này
function showToast(message, type = "success") {
    if (typeof $ === 'undefined' || typeof $.fn.toast === 'undefined') {
        console.error("Lỗi: jQuery hoặc Bootstrap JS (Toast) chưa được tải.");
        console.log(`[Toast ${type}]: ${message}`); 
        return;
    } 
    
    const bgClass = type === "success" ? "bg-success" : (type === "error" ? "bg-danger" : "bg-info");
    const title = type === "success" ? "Thành công" : (type === "error" ? "Lỗi" : "Thông báo");
    
    const html = `
        <div class="toast border-0 ${bgClass} text-white" role="alert" aria-live="assertive" aria-atomic="true" data-delay="3000">
            <div class="toast-header">
                <strong class="mr-auto text-dark">${title}</strong>
                <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    // Đây là container Toast trong HTML của bạn
    $("#toastContainer").append(html); 
    $("#toastContainer .toast").last().toast('show'); 
    $("#toastContainer .toast").last().on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// =========================
// 🔑 Hàm lấy thông tin người dùng (GIẢ ĐỊNH)
// =========================
function getUserId() {
    return 4; 
}

function getAuthToken() {
    // Chỉ để chuỗi Token (bao gồm "Bearer " và token JWT)
    return 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'; 
}

// HOẶC 
function getAuthToken() {
    // Chỉ để chuỗi Token, không có bất kỳ ký tự tiếng Việt nào trong chuỗi này
    return 'Bearer ' + 'TOKEN_THUC_TE_CUA_BAN'; 
}

// =========================
// 🔐 Đổi mật khẩu (PUT)
// =========================
async function doiMatKhau(event) {
    event.preventDefault(); 

    // Kiểm tra và chắc chắn jQuery có sẵn trước khi dùng $
    if (typeof $ === 'undefined') {
        return showToast("Lỗi: Thư viện jQuery chưa được tải.", "error");
    }

    const userId = getUserId();
    const token = getAuthToken();
    
    // Lấy dữ liệu từ form (sử dụng jQuery)
    const matKhauCu = $("#MatKhauCu").val().trim();
    const matKhauMoi = $("#MatKhauMoi").val().trim();
    const xacNhanMatKhauMoi = $("#XacNhanMatKhauMoi").val().trim();

    // Validation
    if (!matKhauCu || !matKhauMoi || !xacNhanMatKhauMoi) {
        return showToast("Vui lòng điền đủ các trường mật khẩu.", "error");
    }

    if (matKhauMoi.length < 6) {
        return showToast("Mật khẩu mới phải có ít nhất 6 ký tự.", "error");
    }
    
    if (matKhauMoi !== xacNhanMatKhauMoi) {
        return showToast("Mật khẩu mới và Xác nhận mật khẩu mới không khớp!", "error");
    }

    // Payload chuẩn bị gửi lên API
    const payload = { MatKhauCu: matKhauCu, MatKhauMoi: matKhauMoi };

    try {
        // Thực hiện request PUT đến API
        await fetchJson(`${taiKhoanBaseUrl}/${userId}/doimatkhau`, {
            method: 'PUT',
            headers: { 
                'Authorization': token,
                'Content-Type': 'application/json' 
            },
            body: JSON.stringify(payload)
        });

        showToast("Đổi mật khẩu thành công!", "success");
        // Xóa form sau khi thành công bằng jQuery
        $("#doiMatKhauForm")[0].reset(); 

    } catch (err) {
        // Hiển thị lỗi từ server
        showToast(`Đổi mật khẩu thất bại: ${err.message}`, "error");
    }
}

// =========================
// ⚡ Gắn sự kiện (Tối ưu hóa bằng JS thuần)
// =========================
document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('doiMatKhauForm');
    if (form) {
        // Gắn sự kiện submit cho form đổi mật khẩu
        form.addEventListener('submit', doiMatKhau); 
    } else {
        console.error("Lỗi: Không tìm thấy form đổi mật khẩu với ID 'doiMatKhauForm'.");
    }
});