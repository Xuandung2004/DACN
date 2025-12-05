; (function () {
    async function applyRoleAdjustments() {
        try {
            let role = '';

            // Try server-side endpoint first
            try {
                const resp = await fetch('/api/auth/me', { credentials: 'include' });
                if (resp.ok) {
                    const data = await resp.json();
                    role = (data.role || '').toLowerCase();
                }
            } catch (e) {
                // ignore fetch errors
            }

            // Fallback: try localStorage (login page saves currentUser)
            if (!role) {
                try {
                    const raw = localStorage.getItem('currentUser') || localStorage.getItem('user') || null;
                    if (raw) {
                        const u = JSON.parse(raw);
                        role = (u.role || u.viTri || u.ViTri || '').toString().toLowerCase();
                    }
                } catch (e) {/* ignore */ }
            }

            if (!role) {
                console.debug('admin-role.js: no role detected');
                return;
            }

            const isEmployee = role.includes('nhan') || role.includes('nhân') || role.includes('nv');
            if (!isEmployee) return;

            // 1) Remove/hide user management link
            const userLink = document.querySelector('a[href="listnguoidung.html"], a[href="/FrontendWeb/admin/listnguoidung.html"]');
            if (userLink) {
                const li = userLink.closest('.nav-item');
                if (li) li.remove();
                else userLink.style.display = 'none';
            }

            // 2) Redirect admin dashboard links to employee dashboard
            document.querySelectorAll('a[href="/FrontendWeb/admin/index.html"], a[href="index.html"], a[href="/FrontendWeb/admin/index.html"]').forEach(a => {
                a.setAttribute('href', '/FrontendWeb/admin/indexnv.html');
            });

            // 3) Inject style to change sidebar color for employee
            if (!document.getElementById('admin-role-employee-style')) {
                const style = document.createElement('style');
                style.id = 'admin-role-employee-style';
                style.textContent = `
                    .bg-gradient-primary {
                        background-color: #1f8a70 !important;
                        background-image: linear-gradient(180deg, #1f8a70 10%, #4cbf84 100%) !important;
                        background-size: cover !important;
                    }
                    .sidebar .nav-item .nav-link,
                    .sidebar .sidebar-brand .sidebar-brand-text,
                    .sidebar .sidebar-brand .sidebar-brand-icon {
                        color: #ffffff !imprtant;
                    }
                `;
                document.head.appendChild(style);
            }

            // 4) Update topbar label from 'Admin' to 'Nhân viên' when employee
            try {
                const headerSpan = document.querySelector('#userDropdown .mr-2') || document.querySelector('#userDropdown span');
                if (headerSpan) headerSpan.textContent = 'Nhân viên';
            } catch (e) {/* ignore */ }

        } catch (err) {
            // silent
            console.warn('admin-role.js error:', err);
        }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', applyRoleAdjustments);
    else applyRoleAdjustments();
})();
