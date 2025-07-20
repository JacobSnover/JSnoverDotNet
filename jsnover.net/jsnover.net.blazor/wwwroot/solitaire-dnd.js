window.solitaireDragDrop = {
    dotNetHelper: null,
    initialized: false
};

window.enableDragDrop = function(dotNetHelper) {
    console.log('Initializing drag and drop...');
    window.solitaireDragDrop.dotNetHelper = dotNetHelper;
    
    function handleDragStart(e) {
        console.log('Drag start:', e.target.dataset.cardid);
        if (e.target.getAttribute('draggable') === 'false') {
            console.log('Card is not draggable');
            e.preventDefault();
            return;
        }
        e.dataTransfer.setData('text/plain', e.target.dataset.cardid);
        e.target.style.opacity = '0.5';
    }

    function handleDragEnd(e) {
        console.log('Drag end');
        e.target.style.opacity = '1';
    }

    function handleDragOver(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
    }

    function handleDragLeave(e) {
        e.preventDefault();
    }

    async function handleDrop(e, type) {
        e.preventDefault();
        try {
            if (!window.solitaireDragDrop.dotNetHelper) {
                console.error('No .NET helper available');
                return;
            }
            const cardId = e.dataTransfer.getData('text/plain');
            const index = parseInt(e.currentTarget.dataset[type === 'foundation' ? 'foundationindex' : 'colindex']);
            console.log('Dropping card', cardId, 'on', type, 'at index', index);
            await window.solitaireDragDrop.dotNetHelper.invokeMethodAsync('OnCardDrop', cardId, type, index);
        } catch (error) {
            console.error('Error in handleDrop:', error);
        }
    }

    function setupDragAndDrop() {
        console.log('Setting up drag and drop handlers...');
        
        // Setup drag events for cards
        const cards = document.querySelectorAll('.tableau-card, .waste-card');
        console.log('Found cards:', cards.length);
        
        cards.forEach(card => {
            const isDraggable = card.getAttribute('draggable') !== 'false';
            console.log('Card:', card.dataset.cardid, 'draggable:', isDraggable);
            if (isDraggable) {
                card.addEventListener('dragstart', handleDragStart);
                card.addEventListener('dragend', handleDragEnd);
            }
        });

        // Setup drop zones
        const tableauZones = document.querySelectorAll('.tableau-dropzone');
        console.log('Found tableau zones:', tableauZones.length);
        tableauZones.forEach(zone => {
            zone.addEventListener('dragover', handleDragOver);
            zone.addEventListener('dragleave', handleDragLeave);
            zone.addEventListener('drop', (e) => handleDrop(e, 'tableau'));
        });

        const foundationZones = document.querySelectorAll('.foundation-dropzone');
        console.log('Found foundation zones:', foundationZones.length);
        foundationZones.forEach(zone => {
            zone.addEventListener('dragover', handleDragOver);
            zone.addEventListener('dragleave', handleDragLeave);
            zone.addEventListener('drop', (e) => handleDrop(e, 'foundation'));
        });
    }

    // Set up mutation observer to handle dynamically added cards
    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node instanceof Element) {
                    const cards = node.classList?.contains('tableau-card') || node.classList?.contains('waste-card') 
                        ? [node] 
                        : node.querySelectorAll('.tableau-card, .waste-card');
                    
                    cards.forEach(card => {
                        if (card.getAttribute('draggable') !== 'false') {
                            card.addEventListener('dragstart', handleDragStart);
                            card.addEventListener('dragend', handleDragEnd);
                        }
                    });
                }
            });
        });
    });

    // Initial setup
    setupDragAndDrop();

    // Start observing the game board
    const gameBoard = document.querySelector('.GameBoard');
    if (gameBoard) {
        observer.observe(gameBoard, { childList: true, subtree: true });
    }
}
