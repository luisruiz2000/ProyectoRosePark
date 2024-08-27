document.addEventListener('DOMContentLoaded', function() {
    // Obtener las referencias a los inputs de fechas
    const checkinDate = document.getElementById('checkinDate');
    const checkoutDate = document.getElementById('checkoutDate');
    const today = new Date().toISOString().split('T')[0]; // Obtener la fecha de hoy en formato 'YYYY-MM-DD'

    // Establecer la fecha mínima de check-in y check-out como la fecha de hoy
    checkinDate.setAttribute('min', today);
    checkoutDate.setAttribute('min', today);

    // Evento para cuando se cambia la fecha de check-in
    checkinDate.addEventListener('change', function() {
        const checkinValue = checkinDate.value;
        const checkoutValue = checkoutDate.value;

        // Establecer la fecha mínima de check-out como la fecha seleccionada en check-in
        checkoutDate.setAttribute('min', checkinValue);

        // Asegurarse de que la fecha de check-out no sea menor que la de check-in
        if (checkoutValue && checkoutValue < checkinValue) {
            alert('La fecha de check-out no puede ser anterior a la de check-in');
            checkoutDate.value = checkinValue;
        }
    });

    // Evento para cuando se cambia la fecha de check-out
    checkoutDate.addEventListener('change', function() {
        const checkoutValue = checkoutDate.value;

        // Asegurarse de que la fecha de check-out no sea menor que la de check-in
        if (checkinDate.value && checkoutValue < checkinDate.value) {
            alert('La fecha de check-out no puede ser anterior a la de check-in');
            checkoutDate.value = checkinDate.value;
        }

        // Establecer la fecha máxima de check-in como la fecha seleccionada en check-out
        checkinDate.setAttribute('max', checkoutValue);
    });
});
