const baseApi = "http://localhost:5150/api/Thongtinnhan";
const userId = localStorage.getItem("userId");

async function loadAddresses() {
    const res = await fetch(`${baseApi}/user/${userId}`);
    const data = await res.json();

    let html = "";
    data.forEach(a => {
        html += `
            <div class="address-item">

                <div class="address-name">
                    ${a.tenNguoiNhan}
                    <span class="address-phone">(+84) ${a.sdtnn}</span>
                </div>

                <div class="address-text">
                    ${a.diaChiNhan}
                </div>

                ${a.isDefault ? `<div class="badge-default">Mặc định</div>` : ""}

                ${a.isOutdated ? `
                    <div class="notice-box">
                        Một vài thông tin đã cũ, vui lòng giúp chúng tôi cập nhật địa chỉ này.
                    </div>
                ` : ""}

                <div class="action-buttons">
                    <a onclick="editAddress(${a.id})">Cập nhật</a>
                    ${!a.isDefault ? `<a onclick="deleteAddress(${a.id})">Xóa</a>` : ""}
                    <div>
                        ${!a.isDefault ? `
                            <button class="set-default-btn" onclick="setDefault(${a.id})">
                                Thiết lập mặc định
                            </button>` : ""}
                    </div>
                </div>

            </div>
        `;
    });

    document.getElementById("address-list").innerHTML = html;
}

loadAddresses();
