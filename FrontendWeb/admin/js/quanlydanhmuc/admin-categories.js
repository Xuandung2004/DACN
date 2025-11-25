// Base API URL
const baseUrl = 'http://localhost:5150/api/DanhMuc';

// Utility function for making API calls
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    if (!res.ok) {
        const text = await res.text();
        throw new Error(`${res.status} ${res.statusText} - ${text}`);
    }
    return res.json();
}

// Load all categories
async function loadDanhMuc() {
    try {
        const categories = await fetchJson(baseUrl);
        const tbody = document.getElementById('danhMucBody');
        tbody.innerHTML = '';

        categories.forEach(c => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${c.id}</td>
                <td>${escapeHtml(c.tenDanhMuc)}</td>
                <td>${escapeHtml(c.moTa || '')}</td>
                <td>${escapeHtml(c.slug || '')}</td>
                <td>${c.soLuongSanPham}</td>
                <td>
                    <button class="btn btn-sm btn-info" onclick="editDanhMuc(${c.id})">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteDanhMuc(${c.id})">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

        // Initialize DataTable with Vietnamese language
        if ($.fn.DataTable.isDataTable('#danhMucTable')) {
            $('#danhMucTable').DataTable().destroy();
        }
        $('#danhMucTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json'
            }
        });
    } catch (err) {
        console.error('Load danh mục failed:', err);
        alert('Không thể tải danh sách danh mục: ' + err.message);
    }
}

// HTML Escape function
function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// Create slug from name (basic implementation)
function createSlug(name) {
    if (!name) return '';
    return name.toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[đĐ]/g, 'd')
        .replace(/[^a-z0-9\s-]/g, '')
        .replace(/\s+/g, '-')
        .replace(/-+/g, '-')
        .trim('-');
}

// Reset form for new category
function openModalForCreate() {
    document.getElementById('modalTitle').innerText = 'Thêm danh mục';
    document.getElementById('danhMucForm').reset();
    document.getElementById('Id').value = '';
}

// Load and edit existing category
async function editDanhMuc(id) {
    try {
        const c = await fetchJson(`${baseUrl}/${id}`);
        document.getElementById('modalTitle').innerText = 'Sửa danh mục';
        document.getElementById('Id').value = c.id;
        document.getElementById('TenDm').value = c.tenDanhMuc;
        document.getElementById('MoTa').value = c.moTa || '';
        document.getElementById('Slug').value = c.slug || '';
        $('#danhMucModal').modal('show');
    } catch (err) {
        console.error('Load danh mục failed:', err);
        alert('Không thể tải thông tin danh mục: ' + err.message);
    }
}

// Save category (create or update)
async function saveDanhMuc() {
    const form = document.getElementById('danhMucForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const id = document.getElementById('Id').value;
    const payload = {
        id: id ? Number(id) : 0,
        tenDm: document.getElementById('TenDm').value.trim(),
        moTa: document.getElementById('MoTa').value.trim(),
        slug: document.getElementById('Slug').value.trim()
    };

    try {
        if (!payload.tenDm) {
            alert('Vui lòng nhập tên danh mục');
            return;
        }

        if (id) {
            // Update
            await fetchJson(`${baseUrl}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        } else {
            // Create
            await fetchJson(baseUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        }

        $('#danhMucModal').modal('hide');
        await loadDanhMuc();

        // Show success message
        alert(id ? 'Cập nhật thành công!' : 'Thêm mới thành công!');
    } catch (err) {
        console.error('Save danh mục failed:', err);
        alert('Lưu thất bại: ' + err.message);
    }
}

// Delete category
async function deleteDanhMuc(id) {
    if (!confirm('Bạn có chắc muốn xóa danh mục này? Nếu danh mục có sản phẩm, bạn sẽ không thể xóa.')) return;

    try {
        await fetchJson(`${baseUrl}/${id}`, { method: 'DELETE' });
        await loadDanhMuc();
        alert('Xóa thành công!');
    } catch (err) {
        console.error('Delete failed:', err);
        alert('Xóa thất bại: ' + err.message);
    }
}

// Auto-generate slug when typing name
function setupSlugGeneration() {
    const nameInput = document.getElementById('TenDm');
    const slugInput = document.getElementById('Slug');

    if (nameInput && slugInput) {
        nameInput.addEventListener('input', function () {
            // Only auto-generate if slug field is empty
            if (!slugInput.value) {
                slugInput.value = createSlug(this.value);
            }
        });
    }
}

// Wire up events
document.addEventListener('DOMContentLoaded', () => {
    // Handle modal reset when showing
    // Only reset when the modal is opened by a DOM element (e.relatedTarget)
    // that does NOT provide a data-id (i.e. the "Thêm danh mục" link/button).
    // If the modal is opened programmatically (e.relatedTarget === undefined)
    // we must NOT reset the form (so edit flow works).
    $('#danhMucModal').on('show.bs.modal', function (e) {
        if (e && e.relatedTarget) {
            // relatedTarget exists (user clicked something to open modal)
            // If it doesn't have a data-id attribute, treat as Create
            const trigger = e.relatedTarget;
            if (!trigger.dataset || !trigger.dataset.id) {
                openModalForCreate();
            }
        }
        // else: opened programmatically for editing - do nothing here
    });

    // Setup slug auto-generation
    setupSlugGeneration();

    // Handle save button click
    const saveBtn = document.getElementById('saveBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveDanhMuc);
    }

    // Load initial data
    loadDanhMuc();
});