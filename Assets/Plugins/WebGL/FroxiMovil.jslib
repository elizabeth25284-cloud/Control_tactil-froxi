// Le dice a Unity si el juego esta abierto en un celular o tablet (para mostrar los controles tactiles)
mergeInto(LibraryManager.library, {
  FroxiEsMovil: function () {
    var ua = navigator.userAgent || "";
    var movil = /Android|iPhone|iPad|iPod|Mobile|Tablet|Silk|Kindle/i.test(ua);
    var ipadNuevo = /Macintosh/i.test(ua) && navigator.maxTouchPoints > 1;
    return (movil || ipadNuevo) ? 1 : 0;
  }
});
