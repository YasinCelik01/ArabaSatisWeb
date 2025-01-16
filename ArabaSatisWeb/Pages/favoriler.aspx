﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="favoriler.aspx.cs" Inherits="ArabaSatisWeb.Pages.favoriler" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/araclarim.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="cars-container">
        <h1></h1>
        <h1>Favorilerim</h1>
        <div id="carsList">
            <!-- Araçlar burada listelenecek -->
        </div>
    </div>

    <script>
        fetch('/api/favorilerim', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                const carsList = document.getElementById('carsList');
                if (data && data.Data) {
                    data.Data.forEach(car => {
                        const carItem = document.createElement('div');
                        carItem.classList.add('car-item');
                        let photosHtml = '';
                        // Araç fotoğraflarını listele
                        if (car.FotografListesi && car.FotografListesi.length > 0) {
                            car.FotografListesi.forEach(photo => {
                                photosHtml += `<img src=" ${photo.FotografUrl}" alt="Car Image" class="car-photo">`;
                            });
                            console.log(car.FotografListesi);
                        } else {
                            photosHtml = '<p>Bu aracın fotoğrafı yok.</p>';
                        }
                        carItem.innerHTML = `
                        <div class="car-header">
                            <h3>${car.Marka} ${car.Model}</h3>
                            <p><strong>Yıl:</strong> ${car.Yil} | <strong>Fiyat:</strong> ${car.Fiyat.toFixed(2)} TL</p>
                        </div>
                        <div class="car-photos">
                            ${photosHtml || '<p>Bu aracın fotoğrafı yok.</p>'}
                        </div>
                        <div class="car-details">
                            <p><strong>Kilometre:</strong> ${car.Kilometre} km</p>
                            <p><strong>Yakıtturu:</strong> ${car.YakitTuru}</p>
                            <p><strong>Vites Türü:</strong> ${car.VitesTuru}</p>
                            <p><strong>Kasa Türü:</strong> ${car.KasaTuru}</p>
                            <p><strong>Motor Hacmi:</strong> ${car.MotorHacmi} L</p>
                            <p><strong>Renk:</strong> ${car.Renk}</p>
                            <p><strong>Telefon:</strong> ${car.Telefon}</p>
                        </div>
                        <div class="car-description">
                            <h4>Açıklama</h4>
                            <p>${car.Aciklama || 'Açıklama bulunmamaktadır.'}</p>
                        </div>
                        <div class="car-footer">
                            <p><strong>Kayit Tarihi:</strong> ${new Date(car.KayitTarihi).toLocaleDateString()}</p>
                            <p><strong>Onay Durumu:</strong> ${car.Onay === 1 ? 'Onaylı' : 'Bekliyor'}</p>
                        </div>
                    `;
                        carsList.appendChild(carItem);
                    });
                } else {
                    carsList.innerHTML = '<p>Henüz bir aracınız yok.</p>';
                }
            })
            .catch(error => {
                console.error('Hata:', error);
                alert('Henüz bir aracınız yok.');
            });

    </script>
</asp:Content>
