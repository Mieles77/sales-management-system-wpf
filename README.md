# Sales Management System - WPF (.NET)

Sistema de gestión de ventas desarrollado como aplicación de escritorio en WPF,
orientado a pequeños y medianos negocios. Integra autenticación, pagos,
persistencia de datos y herramientas administrativas.

## 🧠 Descripción
Este proyecto implementa un sistema completo de gestión de ventas que permite
administrar productos, ventas y usuarios, integrando servicios externos como
autenticación con Google y pasarela de pagos MercadoPago.

Fue desarrollado con el objetivo de simular un sistema real de uso empresarial,
cubriendo tanto lógica de negocio como integración de servicios.

## 🛠️ Tecnologías utilizadas
- C#
- WPF (.NET)
- SQLite
- Google OAuth
- MercadoPago API
- Importación y exportación de datos en Excel

## ⚙️ Funcionalidades
- Autenticación de usuarios mediante Google OAuth
- Gestión de productos y ventas
- Procesamiento de pagos con MercadoPago
- Persistencia de datos con SQLite
- Importación y exportación de información en archivos Excel
- Chatbot integrado para asistencia al usuario

## 🏗️ Arquitectura
Aplicación de escritorio (WPF) → Lógica de negocio → Base de datos SQLite  
Integración con servicios externos vía APIs REST

## 🚀 Ejecución del proyecto
1. Clonar el repositorio
```bash
git clone https://github.com/Mieles77/wpf-sales-management-system.git
