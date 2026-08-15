function InitSwiperPhones() {
  var swiper = new Swiper('.swiper-container', {
    pagination: {
      el: '.swiper-pagination',
      dynamicBullets: true,
    },
  });

  Element.prototype.hasClassNamePH = function(name) {
    return new RegExp("(?:^|\\s+)" + name + "(?:\\s+|$)").test(this.className);
  };

  Element.prototype.addClassNamePH = function(name) {
    if (!this.hasClassNamePH(name)) {
      this.className = this.className ? [this.className, name].join(' ') : name;
    }
  };

  Element.prototype.removeClassNamePH = function(name) {
    if (this.hasClassNamePH(name)) {
      var c = this.className;
      this.className = c.replace(new RegExp("(?:^|\\s+)" + name + "(?:\\s+|$)", "g"), "");
    }
  };
  
  var samples = samples || {};

  // dragStart
  (function() {
    var id_ = 'columns-dragStart';
    var cols_ = document.querySelectorAll('#' + id_ + ' .column');

    this.handleDragStart = function(e) {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/html', 'blah'); // needed for FF.

      // Target element (this) is the source node.
      this.style.opacity = '0.4';
    };

    [].forEach.call(cols_, function (col) {
      // Enable columns to be draggable.
      col.setAttribute('draggable', 'true');
      col.addEventListener('dragstart', this.handleDragStart, false);
    });

  })();

  // dragEnd
  (function() {
    var id_ = 'columns-dragEnd';
    var cols_ = document.querySelectorAll('#' + id_ + ' .column');

    this.handleDragStart = function(e) {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/html', this.innerHTML); // needed for FF.

      // Target element (this) is the source node.
      this.style.opacity = '0.4';
    };

    this.handleDragOver = function(e) {
      if (e.preventDefault) {
        e.preventDefault(); // Allows us to drop.
      }

      e.dataTransfer.dropEffect = 'move';

      return false;
    };

    this.handleDragEnter = function(e) {
      this.addClassNamePH('over');
    };

    this.handleDragLeave = function(e) {
      // this/e.target is previous target element.

      this.removeClassNamePH('over');
    };

    this.handleDragEnd = function(e) {
      [].forEach.call(cols_, function (col) {
        col.removeClassNamePH('over');
      });

      // target element (this) is the source node.
      this.style.opacity = '1';
    };

    [].forEach.call(cols_, function (col) {
      // Enable columns to be draggable.
      col.setAttribute('draggable', 'true');
      col.addEventListener('dragstart', this.handleDragStart, false);
      col.addEventListener('dragenter', this.handleDragEnter, false);
      col.addEventListener('dragover', this.handleDragOver, false);
      col.addEventListener('dragleave', this.handleDragLeave, false);
      col.addEventListener('dragend', this.handleDragEnd, false);
    });

  })();

  // dragIcon
  (function() {
    var id_ = 'columns-dragIcon';
    var cols_ = document.querySelectorAll('#' + id_ + ' .column');

    this.handleDragStart = function(e) {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/html', this.innerHTML);

      var dragIcon = document.createElement('img');
      dragIcon.src = '/static/images/google_logo_small.png';
      e.dataTransfer.setDragImage(dragIcon, -10, -10);

      // Target element (this) is the source node.
      this.style.opacity = '0.4';
    };

    this.handleDragLeave = function(e) {
      // this/e.target is previous target element.

      this.removeClassNamePH('over');
    };

    this.handleDragEnd = function(e) {
      // this/e.target is the source node.

      this.style.opacity = '1';

      [].forEach.call(cols_, function (col) {
        col.removeClassNamePH('over');
      });
    };

    [].forEach.call(cols_, function (col) {
      // Enable columns to be draggable.
      col.setAttribute('draggable', 'true');
      col.addEventListener('dragstart', this.handleDragStart, false);
      col.addEventListener('dragend', this.handleDragEnd, false);
      col.addEventListener('dragleave', this.handleDragLeave, false);
    });

  })();

  // Almost final example
  (function() {
    var id_ = 'columns-almostFinal';
    var cols_ = document.querySelectorAll('#' + id_ + ' .column');
    var dragSrcEl_ = null;

    this.handleDragStart = function(e) {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/html', this.innerHTML);

      dragSrcEl_ = this;

      this.style.opacity = '0.4';

      // this/e.target is the source node.
      this.addClassNamePH('moving');
    };

    this.handleDragOver = function(e) {
      if (e.preventDefault) {
        e.preventDefault(); // Allows us to drop.
      }

      e.dataTransfer.dropEffect = 'move';

      return false;
    };

    this.handleDragEnter = function(e) { // HERE

      this.addClassNamePH('over');
    };

    this.handleDragLeave = function(e) {
      // this/e.target is previous target element.

      this.removeClassNamePH('over');
    };

    this.handleDrop = function(e) {
      // this/e.target is current target element.

      if (e.stopPropagation) {
        e.stopPropagation(); // stops the browser from redirecting.
      }

      // Don't do anything if we're dropping on the same column we're dragging.
      if (dragSrcEl_ != this) {
        dragSrcEl_.innerHTML = this.innerHTML;
        this.innerHTML = e.dataTransfer.getData('text/html');
      }

      return false;
    };

    this.handleDragEnd = function(e) { // HERE
      // this/e.target is the source node.
      this.style.opacity = '1';

      [].forEach.call(cols_, function (col) {
        col.removeClassNamePH('over');
        col.removeClassNamePH('moving');
      });
    };

    [].forEach.call(cols_, function (col) {
      col.setAttribute('draggable', 'true');  // Enable columns to be draggable.
      col.addEventListener('dragstart', this.handleDragStart, false);
      col.addEventListener('dragenter', this.handleDragEnter, false);
      col.addEventListener('dragover', this.handleDragOver, false);
      col.addEventListener('dragleave', this.handleDragLeave, false);
      col.addEventListener('drop', this.handleDrop, false);
      col.addEventListener('dragend', this.handleDragEnd, false);
    });
  })();

  // Full example
  (function() {
    var id_ = 'columns-full';
    var cols_ = document.querySelectorAll('#' + id_ + ' .column');
    var dragSrcEl_ = null;

    this.handleDragStart = function(e) { // Comienza a arrastrar
      this.style.webkitTransform += "scale(0.8)";
      this.style.opacity = 0.25;

      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/html', this.innerHTML);

      dragSrcEl_ = this;

      // this/e.target is the source node.
      this.addClassNamePH('moving');
    };

    this.handleDragOver = function(e) { // HERE
      
      if (e.preventDefault) {
        e.preventDefault(); // Allows us to drop.
      }



      e.dataTransfer.dropEffect = 'move';

      return false;
    };

    this.handleDragEnter = function(e) {// Poner app encima de otra
      this.addClassNamePH('over');
    };

    this.handleDragLeave = function(e) { // Soltar APP
      // this/e.target is previous target element.

      this.removeClassNamePH('over');
    };

    this.handleDrop = function(e) {
      // this/e.target is current target element.

      if (e.stopPropagation) {
        e.stopPropagation(); // stops the browser from redirecting.
      }

      // Don't do anything if we're dropping on the same column we're dragging.
      if (dragSrcEl_ != this) {
        dragSrcEl_.innerHTML = this.innerHTML;
        this.innerHTML = e.dataTransfer.getData('text/html');

        // Set number of times the column has been moved.
        var count = this.querySelector('.count');
        var newCount = parseInt(count.getAttribute('data-col-moves')) + 1;
        count.setAttribute('data-col-moves', newCount);
        //count.textContent = 'moves: ' + newCount;
      }

      return false;
    };

    this.handleDragEnd = function(e) {
      // this/e.target is the source node.
      this.style.webkitTransform = "scale(1)";
      this.style.opacity = 1;

      [].forEach.call(cols_, function (col) {
        col.removeClassNamePH('over');
        col.removeClassNamePH('moving');
      });
    };

    [].forEach.call(cols_, function (col) {
      col.setAttribute('draggable', 'true');  // Enable columns to be draggable.
      col.addEventListener('dragstart', this.handleDragStart, false);
      col.addEventListener('dragenter', this.handleDragEnter, false);
      col.addEventListener('dragover', this.handleDragOver, false);
      col.addEventListener('dragleave', this.handleDragLeave, false);
      col.addEventListener('drop', this.handleDrop, false);
      col.addEventListener('dragend', this.handleDragEnd, false);
    });
  })();
}

$(function() {
  // Initializes and creates emoji set from sprite sheet
  window.emojiPicker = new EmojiPicker({
    emojiable_selector: '[data-emojiable=true]',
    assetsPath: '/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/emojis/lib/img/',
    popupButtonClasses: 'fa fa-smile-o'
  });
  // Finds all elements with `emojiable_selector` and converts them to rich emoji input fields
  // You may want to delay this step if you have dynamically created input fields that appear later in the loading process
  // It can be called as many times as necessary; previously converted input fields will not be converted again
  window.emojiPicker.discover();
});

function RefreshEmojis(){  
  // Initializes and creates emoji set from sprite sheet
  window.emojiPicker = new EmojiPicker({
    emojiable_selector: '[data-emojiable=true]',
    assetsPath: '/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/emojis/lib/img/',
    popupButtonClasses: 'fa fa-smile-o'
  });
  // Finds all elements with `emojiable_selector` and converts them to rich emoji input fields
  // You may want to delay this step if you have dynamically created input fields that appear later in the loading process
  // It can be called as many times as necessary; previously converted input fields will not be converted again
  window.emojiPicker.discover();
}

function Unavailable($MSG){
  $('#phone_error_msg').html($MSG);
  $('#phone_error').show();
  $('#phone_msg_box').addClass('animated zoomIn');
  $('#phone_msg_box').get(0).style.display = "inherit";
}

$('#phone_error_button').click(function() {
  $('#phone_error').hide();
  $('#phone_msg_box').hide();
});