document.addEventListener('DOMContentLoaded', function () {
    // Obtener las referencias a los inputs de fechas
    const checkinDate = document.getElementById('checkinDate');
    const checkoutDate = document.getElementById('checkoutDate');
    const today = new Date().toISOString().split('T')[0]; // Obtener la fecha de hoy en formato 'YYYY-MM-DD'

    // Establecer la fecha mínima de check-in y check-out como la fecha de hoy
    checkinDate.setAttribute('min', today);
    checkoutDate.setAttribute('min', today);

    // Función para actualizar las fechas mínimas
    function updateMinCheckoutDate() {
        const checkinValue = checkinDate.value;
        if (checkinValue) {
            const minCheckoutDate = new Date(checkinValue);
            minCheckoutDate.setDate(minCheckoutDate.getDate() + 1); // Añadir un día
            checkoutDate.setAttribute('min', minCheckoutDate.toISOString().split('T')[0]);

            // Si la fecha de check-out es antes de la nueva fecha mínima, actualizarla
            if (checkoutDate.value && checkoutDate.value < minCheckoutDate.toISOString().split('T')[0]) {
                checkoutDate.value = minCheckoutDate.toISOString().split('T')[0];
            }
        }
    }

    // Evento para cuando se cambia la fecha de check-in
    checkinDate.addEventListener('change', function () {
        updateMinCheckoutDate();
        // Si la fecha de check-out es antes de la fecha de check-in, actualizarla
        if (checkoutDate.value && checkoutDate.value <= checkinDate.value) {
            alert('La fecha de check-out debe ser al menos un día después de la fecha de check-in.');
            checkoutDate.value = '';
        }
    });

    // Evento para cuando se cambia la fecha de check-out
    checkoutDate.addEventListener('change', function () {
        // Asegurarse de que la fecha de check-out no sea menor que la de check-in
        if (checkinDate.value && checkoutDate.value <= checkinDate.value) {
            alert('La fecha de check-out debe ser al menos un día después de la fecha de check-in.');
            checkoutDate.value = '';
        }
    });
});
