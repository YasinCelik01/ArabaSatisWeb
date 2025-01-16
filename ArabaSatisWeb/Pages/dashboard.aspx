<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="dashboard.aspx.cs" Inherits="ArabaSatisWeb.Pages.dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/dashboard.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <div class="admin-dashboard-container">
        <h1>Admin Dashboard</h1>
        <div class="car-list-container">
            <h2>Onay Bekleyen Araçlar</h2>
            <table id="carListTable">
                <thead>
                    <tr>
                        <th>marka</th>
                        <th>model</th>
                        <th>Yıl</th>
                        <th>İşlem</th>
                    </tr>
                </thead>
                <tbody>
                    <!-- Onay bekleyen araçlar buraya eklenecek -->
                </tbody>
            </table>
        </div>
    </div>

    <script>
        // Onay bekleyen araçları yükleyen fonksiyon
        fetch('/api/admin/dashboard', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                'Content-Type': 'application/json'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data && data.Data) {
                const carListTable = document.getElementById('carListTable').getElementsByTagName('tbody')[0];
                data.Data.forEach(car => {
                    const row = carListTable.insertRow();
                    row.innerHTML = `
                        <td>${car.Marka}</td>
                        <td>${car.Model}</td>
                        <td>${car.Yil}</td>
                        <td>
                            <button onclick="approveCar(${car.ArabaId})" class="btn btn-approve">Onayla</button>
                            <button onclick="rejectCar(${car.ArabaId})" class="btn btn-reject">Reddet</button>
                        </td>
                    `;
                });
            } else {
                alert('Onay bekleyen araç bulunmamaktadır.');
            }
        })
        .catch(error => {
            console.error('Hata:', error);
            alert('Araç listesi yüklenirken bir hata oluştu.');
        });

        // Onayla butonuna basıldığında
        function approveCar(carId) {
            fetch('/api/admin/approveCar', {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ carId: carId, status: 1 })
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(errorData.Message || 'Bir hata oluştu');
                        });
                    }
                    return response.json();  // Başarılı ise JSON verisini çözümle
                })
                .then(data => {
                    if (data && data.Message === "Araç durumu başarıyla güncellendi.") {
                        alert('Araç onaylandı.');
                        location.reload(); // Sayfayı yenileyerek listeyi güncelle
                    } else {
                        alert(data.Message || 'Bilinmeyen bir hata oluştu.');
                    }
                })
                .catch(error => {
                    console.error('Hata:', error);
                    alert('Onay işlemi sırasında bir hata oluştu: ' + error.message);
                });
        }

        // Reddet butonuna basıldığında
        function rejectCar(carId) {
            fetch('/api/admin/approveCar', {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ carId: carId, status: 0 })
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(errorData.Message || 'Bir hata oluştu');
                        });
                    }
                    return response.json();  // Başarılı ise JSON verisini çözümle
                })
                .then(data => {
                    if (data && data.Message === "Araç durumu başarıyla güncellendi.") {
                        alert('Araç reddedildi.');
                        location.reload(); // Sayfayı yenileyerek listeyi güncelle
                    } else {
                        alert(data.Message || 'Reddetme işlemi başarısız oldu.');
                    }
                })
                .catch(error => {
                    console.error('Hata:', error);
                    alert('Reddetme işlemi sırasında bir hata oluştu: ' + error.message);
                });
        }
    </script>
</asp:Content>
