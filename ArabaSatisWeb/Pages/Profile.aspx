<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="ArabaSatisWeb.Pages.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/Profile.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="profile-container">
        <div class="profile-card">
            <div class="profile-image">
                <img id="profileImg" src="../Images/default-profile.png" alt="Profil Resmi">
            </div>
            <div class="profile-info">
                <h2 id="username">Kullanıcı Adı</h2>
                <p id="email">E-posta: example@example.com</p>
                <p id="role">Rol: Kullanıcı</p>
                <p id="createdDate">Hesap Oluşturma Tarihi: 2024-01-01</p>
                <button onclick="editProfile()" class="btn">Profili Düzenle</button>
                <button onclick="deleteAccount()" class="btn btn-delete">Hesabımı Sil</button>
                <!-- Araçlarım Butonu -->
                <button onclick="goToMyCars()" class="btn">Araçlarım</button>
                <button onclick="goToMyFavori()" class="btn">Favorilerim</button>
                <!-- İlan Ver Butonu -->
                <button onclick="goToPostAd()" class="btn">İlan Ver</button>
                <!-- Admin Panel Butonu (Admin kullanıcı için görünür olacak) -->
                <button id="adminPanelBtn" onclick="goToAdminPanel()" class="btn btn-admin" style="display:none;">Admin Panel</button>
            </div>
        </div>
    </div>

    <script>
        fetch('/api/kullanici/getprofile', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data && data.Data) {
                    document.getElementById('profileImg').src = data.Data.ProfileImageURL || '../Images/default-profile.png';
                    document.getElementById('username').textContent = data.Data.Username;
                    document.getElementById('email').textContent = "E-posta: " + data.Data.Email;
                    document.getElementById('role').textContent = "Rol: " + data.Data.Role;
                    document.getElementById('createdDate').textContent = "Hesap Oluşturma Tarihi: " + data.Data.CreatedDate.split('T')[0];

                    // Admin rolü kontrolü
                    if (data.Data.Role && data.Data.Role.toLowerCase() === 'admin') {
                        document.getElementById('adminPanelBtn').style.display = 'inline-block'; // Adminse butonu göster
                    }
                } else {
                    alert('Profil bilgileri alınamadı.');
                }
            })
            .catch(error => {
                console.error('Hata:', error);
                alert('Profil yüklenirken bir hata oluştu.');
            });

        function editProfile() {
            window.location.href = "editprofile.aspx";
        }

        function deleteAccount() {
            if (confirm("Hesabınızı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.")) {
                fetch('/api/kullanici/delete', {
                    method: 'DELETE',
                    headers: {
                        'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                        'Content-Type': 'application/json'
                    }
                })
                    .then(response => {
                        if (response.ok) {
                            alert('Hesabınız başarıyla silindi.');
                            window.location.href = "login.aspx";
                        } else {
                            alert('Hesap silme işlemi başarısız oldu.');
                        }
                    })
                    .catch(error => {
                        console.error('Hata:', error);
                        alert('Hesap silinirken bir hata oluştu.');
                    });
            }
        }

        function goToAdminPanel() {
            // Admin paneline yönlendirme
            window.location.href = "admin.aspx"; // Admin panelinin URL'si
        }

        function goToMyCars() {
            // Araçlarım sayfasına yönlendirme
            window.location.href = "araclarim.aspx"; // Araçlarım sayfasının URL'si
        }
        function goToMyFavori() {
            // Araçlarım sayfasına yönlendirme
            window.location.href = "favoriler.aspx"; // Araçlarım sayfasının URL'si
        }

        function goToPostAd() {
            // İlan Ver sayfasına yönlendirme
            window.location.href = "ilanver.aspx"; // İlan Ver sayfasının URL'si
        }
    </script>
</asp:Content>
