window.enableDragDrop = function(dotNetHelper) {
    try {
        console.log('Enabling drag and drop...');
        
        // Helper function to setup drag and drop
        function setupDragAndDrop() {
            // Cards
            console.log('Looking for cards...');
            const cards = document.querySelectorAll('.tableau-card, .waste-card');
            console.log('Found', cards.length, 'cards');
            
            cards.forEach(card => {
                console.log('Setting up card:', card.dataset.cardid);
                card.setAttribute('draggable', 'true');
                card.addEventListener('dragstart', e => {
                    console.log('Drag start:', card.dataset.cardid);
                    e.dataTransfer.setData('text/plain', card.dataset.cardid);
                    // Add visual feedback
                    e.target.style.opacity = '0.5';
                });
                card.addEventListener('dragend', e => {
                    e.target.style.opacity = '1';
                });
            });

            // Tableau dropzones
            const tableauZones = document.querySelectorAll('.tableau-dropzone');
            console.log('Found', tableauZones.length, 'tableau zones');
            
            tableauZones.forEach(zone => {
                zone.addEventListener('dragover', e => {
                    e.preventDefault();
                    e.target.closest('.tableau-dropzone').style.backgroundColor = '#225522';
                });
                zone.addEventListener('dragleave', e => {
                    e.preventDefault();
                    e.target.closest('.tableau-dropzone').style.backgroundColor = '#184d18';
                });
                zone.addEventListener('drop', async e => {
                    e.preventDefault();
                    e.target.closest('.tableau-dropzone').style.backgroundColor = '#184d18';
                    const cardId = e.dataTransfer.getData('text/plain');
                    const colIndex = parseInt(zone.dataset.colindex, 10);
                    console.log('Dropping card', cardId, 'on tableau', colIndex);
                    try {
                        await dotNetHelper.invokeMethodAsync('OnCardDrop', cardId, 'tableau', colIndex);
                    } catch (error) {
                        console.error('Error in OnCardDrop:', error);
                    }
                });
            });

            // Foundation dropzones
            const foundationZones = document.querySelectorAll('.foundation-dropzone');
            console.log('Found', foundationZones.length, 'foundation zones');
            
            foundationZones.forEach(zone => {
                zone.addEventListener('dragover', e => {
                    e.preventDefault();
                    e.target.closest('.foundation-dropzone').style.backgroundColor = '#333';
                });
                zone.addEventListener('dragleave', e => {
                    e.preventDefault();
                    e.target.closest('.foundation-dropzone').style.backgroundColor = '#222';
                });
                zone.addEventListener('drop', async e => {
                    e.preventDefault();
                    e.target.closest('.foundation-dropzone').style.backgroundColor = '#222';
                    const cardId = e.dataTransfer.getData('text/plain');
                    const foundationIndex = parseInt(zone.dataset.foundationindex, 10);
                    console.log('Dropping card', cardId, 'on foundation', foundationIndex);
                    try {
                        await dotNetHelper.invokeMethodAsync('OnCardDrop', cardId, 'foundation', foundationIndex);
                    } catch (error) {
                        console.error('Error in OnCardDrop:', error);
                    }
                });
            });
        }

        // Initial setup
        setupDragAndDrop();

        // Setup observer for dynamic updates
        const observer = new MutationObserver((mutations) => {
            console.log('DOM updated, re-enabling drag and drop');
            setupDragAndDrop();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

    } catch (error) {
        console.error('Error in enableDragDrop:', error);
    }
}
