@echo off
chcp 65001 >nul
echo ============================================
echo   Compactador de SVGs para Manual FrotiX
echo ============================================
echo.

REM Criar pasta temporária
if exist "svgs_manual" rd /s /q "svgs_manual"
mkdir svgs_manual

echo Copiando SVGs necessários...
echo.

REM === LOGOS ===
echo [LOGOS]
if exist "logo-frotix.svg" copy "logo-frotix.svg" "svgs_manual\" >nul && echo   + logo-frotix.svg
if exist "logo-frotix-web.svg" copy "logo-frotix-web.svg" "svgs_manual\" >nul && echo   + logo-frotix-web.svg
if exist "logo-economildos.svg" copy "logo-economildos.svg" "svgs_manual\" >nul && echo   + logo-economildos.svg
if exist "logo-vistorias.svg" copy "logo-vistorias.svg" "svgs_manual\" >nul && echo   + logo-vistorias.svg
if exist "logo-camara.svg" copy "logo-camara.svg" "svgs_manual\" >nul && echo   + logo-camara.svg

REM === ÍCONES PRINCIPAIS DO MANUAL ===
echo.
echo [ICONES PRINCIPAIS]

REM Agenda
if exist "calendar-days.svg" copy "calendar-days.svg" "svgs_manual\" >nul && echo   + calendar-days.svg
if exist "calendar.svg" copy "calendar.svg" "svgs_manual\" >nul && echo   + calendar.svg

REM Viagens
if exist "route.svg" copy "route.svg" "svgs_manual\" >nul && echo   + route.svg
if exist "road.svg" copy "road.svg" "svgs_manual\" >nul && echo   + road.svg

REM Dashboards
if exist "chart-mixed.svg" copy "chart-mixed.svg" "svgs_manual\" >nul && echo   + chart-mixed.svg
if exist "chart-line.svg" copy "chart-line.svg" "svgs_manual\" >nul && echo   + chart-line.svg
if exist "chart-pie.svg" copy "chart-pie.svg" "svgs_manual\" >nul && echo   + chart-pie.svg
if exist "gauge-high.svg" copy "gauge-high.svg" "svgs_manual\" >nul && echo   + gauge-high.svg

REM Abastecimento
if exist "gas-pump.svg" copy "gas-pump.svg" "svgs_manual\" >nul && echo   + gas-pump.svg

REM Economildos
if exist "trophy.svg" copy "trophy.svg" "svgs_manual\" >nul && echo   + trophy.svg
if exist "medal.svg" copy "medal.svg" "svgs_manual\" >nul && echo   + medal.svg
if exist "ranking-star.svg" copy "ranking-star.svg" "svgs_manual\" >nul && echo   + ranking-star.svg
if exist "leaf.svg" copy "leaf.svg" "svgs_manual\" >nul && echo   + leaf.svg
if exist "piggy-bank.svg" copy "piggy-bank.svg" "svgs_manual\" >nul && echo   + piggy-bank.svg

REM Manutenções
if exist "wrench.svg" copy "wrench.svg" "svgs_manual\" >nul && echo   + wrench.svg
if exist "screwdriver-wrench.svg" copy "screwdriver-wrench.svg" "svgs_manual\" >nul && echo   + screwdriver-wrench.svg

REM Autuações/Multas
if exist "file-invoice.svg" copy "file-invoice.svg" "svgs_manual\" >nul && echo   + file-invoice.svg
if exist "file-invoice-dollar.svg" copy "file-invoice-dollar.svg" "svgs_manual\" >nul && echo   + file-invoice-dollar.svg
if exist "ticket.svg" copy "ticket.svg" "svgs_manual\" >nul && echo   + ticket.svg

REM Contratos
if exist "file-contract.svg" copy "file-contract.svg" "svgs_manual\" >nul && echo   + file-contract.svg
if exist "file-signature.svg" copy "file-signature.svg" "svgs_manual\" >nul && echo   + file-signature.svg

REM Notas Fiscais
if exist "receipt.svg" copy "receipt.svg" "svgs_manual\" >nul && echo   + receipt.svg
if exist "file-lines.svg" copy "file-lines.svg" "svgs_manual\" >nul && echo   + file-lines.svg

REM Veículos
if exist "car.svg" copy "car.svg" "svgs_manual\" >nul && echo   + car.svg
if exist "car-side.svg" copy "car-side.svg" "svgs_manual\" >nul && echo   + car-side.svg
if exist "bus.svg" copy "bus.svg" "svgs_manual\" >nul && echo   + bus.svg
if exist "truck.svg" copy "truck.svg" "svgs_manual\" >nul && echo   + truck.svg

REM Alertas
if exist "bell.svg" copy "bell.svg" "svgs_manual\" >nul && echo   + bell.svg
if exist "bell-exclamation.svg" copy "bell-exclamation.svg" "svgs_manual\" >nul && echo   + bell-exclamation.svg
if exist "triangle-exclamation.svg" copy "triangle-exclamation.svg" "svgs_manual\" >nul && echo   + triangle-exclamation.svg

REM Mobile
if exist "mobile.svg" copy "mobile.svg" "svgs_manual\" >nul && echo   + mobile.svg
if exist "mobile-screen.svg" copy "mobile-screen.svg" "svgs_manual\" >nul && echo   + mobile-screen.svg
if exist "mobile-screen-button.svg" copy "mobile-screen-button.svg" "svgs_manual\" >nul && echo   + mobile-screen-button.svg

REM Vistorias
if exist "clipboard-check.svg" copy "clipboard-check.svg" "svgs_manual\" >nul && echo   + clipboard-check.svg
if exist "clipboard-list.svg" copy "clipboard-list.svg" "svgs_manual\" >nul && echo   + clipboard-list.svg
if exist "clipboard-list-check.svg" copy "clipboard-list-check.svg" "svgs_manual\" >nul && echo   + clipboard-list-check.svg

REM === ÍCONES ADICIONAIS ===
echo.
echo [ICONES ADICIONAIS]

REM Arquitetura/Integrações
if exist "network-wired.svg" copy "network-wired.svg" "svgs_manual\" >nul && echo   + network-wired.svg
if exist "diagram-project.svg" copy "diagram-project.svg" "svgs_manual\" >nul && echo   + diagram-project.svg
if exist "sitemap.svg" copy "sitemap.svg" "svgs_manual\" >nul && echo   + sitemap.svg
if exist "server.svg" copy "server.svg" "svgs_manual\" >nul && echo   + server.svg
if exist "database.svg" copy "database.svg" "svgs_manual\" >nul && echo   + database.svg
if exist "cloud.svg" copy "cloud.svg" "svgs_manual\" >nul && echo   + cloud.svg

REM Manual/Documentação
if exist "book.svg" copy "book.svg" "svgs_manual\" >nul && echo   + book.svg
if exist "book-open.svg" copy "book-open.svg" "svgs_manual\" >nul && echo   + book-open.svg
if exist "file-pdf.svg" copy "file-pdf.svg" "svgs_manual\" >nul && echo   + file-pdf.svg

REM Usuários/Motoristas
if exist "users.svg" copy "users.svg" "svgs_manual\" >nul && echo   + users.svg
if exist "user.svg" copy "user.svg" "svgs_manual\" >nul && echo   + user.svg
if exist "id-card.svg" copy "id-card.svg" "svgs_manual\" >nul && echo   + id-card.svg
if exist "user-helmet-safety.svg" copy "user-helmet-safety.svg" "svgs_manual\" >nul && echo   + user-helmet-safety.svg

REM Extras úteis
if exist "circle-check.svg" copy "circle-check.svg" "svgs_manual\" >nul && echo   + circle-check.svg
if exist "circle-xmark.svg" copy "circle-xmark.svg" "svgs_manual\" >nul && echo   + circle-xmark.svg
if exist "circle-info.svg" copy "circle-info.svg" "svgs_manual\" >nul && echo   + circle-info.svg
if exist "circle-exclamation.svg" copy "circle-exclamation.svg" "svgs_manual\" >nul && echo   + circle-exclamation.svg
if exist "check.svg" copy "check.svg" "svgs_manual\" >nul && echo   + check.svg
if exist "xmark.svg" copy "xmark.svg" "svgs_manual\" >nul && echo   + xmark.svg
if exist "clock.svg" copy "clock.svg" "svgs_manual\" >nul && echo   + clock.svg
if exist "stopwatch.svg" copy "stopwatch.svg" "svgs_manual\" >nul && echo   + stopwatch.svg
if exist "calendar-check.svg" copy "calendar-check.svg" "svgs_manual\" >nul && echo   + calendar-check.svg
if exist "money-bill.svg" copy "money-bill.svg" "svgs_manual\" >nul && echo   + money-bill.svg
if exist "money-check-dollar.svg" copy "money-check-dollar.svg" "svgs_manual\" >nul && echo   + money-check-dollar.svg
if exist "calculator.svg" copy "calculator.svg" "svgs_manual\" >nul && echo   + calculator.svg
if exist "map-location-dot.svg" copy "map-location-dot.svg" "svgs_manual\" >nul && echo   + map-location-dot.svg
if exist "location-dot.svg" copy "location-dot.svg" "svgs_manual\" >nul && echo   + location-dot.svg
if exist "gear.svg" copy "gear.svg" "svgs_manual\" >nul && echo   + gear.svg
if exist "gears.svg" copy "gears.svg" "svgs_manual\" >nul && echo   + gears.svg
if exist "sliders.svg" copy "sliders.svg" "svgs_manual\" >nul && echo   + sliders.svg
if exist "filter.svg" copy "filter.svg" "svgs_manual\" >nul && echo   + filter.svg
if exist "magnifying-glass.svg" copy "magnifying-glass.svg" "svgs_manual\" >nul && echo   + magnifying-glass.svg
if exist "download.svg" copy "download.svg" "svgs_manual\" >nul && echo   + download.svg
if exist "upload.svg" copy "upload.svg" "svgs_manual\" >nul && echo   + upload.svg
if exist "print.svg" copy "print.svg" "svgs_manual\" >nul && echo   + print.svg
if exist "arrow-right.svg" copy "arrow-right.svg" "svgs_manual\" >nul && echo   + arrow-right.svg
if exist "arrow-left.svg" copy "arrow-left.svg" "svgs_manual\" >nul && echo   + arrow-left.svg
if exist "arrows-rotate.svg" copy "arrows-rotate.svg" "svgs_manual\" >nul && echo   + arrows-rotate.svg
if exist "plus.svg" copy "plus.svg" "svgs_manual\" >nul && echo   + plus.svg
if exist "pen-to-square.svg" copy "pen-to-square.svg" "svgs_manual\" >nul && echo   + pen-to-square.svg
if exist "trash.svg" copy "trash.svg" "svgs_manual\" >nul && echo   + trash.svg
if exist "eye.svg" copy "eye.svg" "svgs_manual\" >nul && echo   + eye.svg
if exist "camera.svg" copy "camera.svg" "svgs_manual\" >nul && echo   + camera.svg
if exist "image.svg" copy "image.svg" "svgs_manual\" >nul && echo   + image.svg
if exist "building.svg" copy "building.svg" "svgs_manual\" >nul && echo   + building.svg
if exist "building-columns.svg" copy "building-columns.svg" "svgs_manual\" >nul && echo   + building-columns.svg
if exist "house.svg" copy "house.svg" "svgs_manual\" >nul && echo   + house.svg
if exist "flag.svg" copy "flag.svg" "svgs_manual\" >nul && echo   + flag.svg
if exist "star.svg" copy "star.svg" "svgs_manual\" >nul && echo   + star.svg
if exist "stars.svg" copy "stars.svg" "svgs_manual\" >nul && echo   + stars.svg
if exist "bolt.svg" copy "bolt.svg" "svgs_manual\" >nul && echo   + bolt.svg
if exist "lightbulb.svg" copy "lightbulb.svg" "svgs_manual\" >nul && echo   + lightbulb.svg
if exist "code.svg" copy "code.svg" "svgs_manual\" >nul && echo   + code.svg
if exist "chart-line-up.svg" copy "chart-line-up.svg" "svgs_manual\" >nul && echo   + chart-line-up.svg

echo.
echo ============================================

REM Contar arquivos copiados
for /f %%a in ('dir /b "svgs_manual\*.svg" 2^>nul ^| find /c /v ""') do set COUNT=%%a
echo   Total de arquivos: %COUNT%
echo ============================================
echo.

REM Criar arquivo TAR
echo Gerando svgs_manual_frotix.tar...
tar -cvf svgs_manual_frotix.tar -C svgs_manual .

echo.
echo ============================================
echo   CONCLUIDO!
echo   Arquivo: svgs_manual_frotix.tar
echo ============================================
echo.

REM Limpar pasta temporária
rd /s /q "svgs_manual"

pause