Ext.require(["Ext.app.Application", "Ext.Component", "Ext.Widget"]);
Ext.require("Ext.reactor.RendererCell");
Ext.create({
  xtype: 'container',
  padding: "20"
});
Ext.create({"xtype":"container"});
Ext.require('Ext.plugin.Responsive');
Ext.create({"xtype":"formpanel"});
Ext.create({
  xtype: 'numberfield',
  ref: function (field) {
    return _this.numberField = field;
  },
  label: "Value:",
  value: value
});
Ext.create({
  xtype: 'button',
  text: "Edit",
  handler: function () {
    return onEditClick(_this.numberField.getValue());
  }
});
Ext.create({
  xtype: 'button',
  text: "Cancel",
  handler: onCancelClick
});
Ext.create({"xtype":"formpanel"});
Ext.create({"xtype":"numberfield"});
Ext.create({"xtype":"button"});
Ext.create({
  xtype: 'dialog',
  displayed: isVisible,
  title: "Edit Cells",
  closeAction: "hide",
  ref: function (dialog) {
    return _this.dialog = dialog;
  }
});
Ext.create({"xtype":"dialog"});
Ext.create('Ext.data.Store', {
  fields: ['name', 'email', 'phone', 'hoursTaken', 'hoursRemaining'],
  data: data
});
Ext.create({
  xtype: 'grid',
  store: this.store
});
Ext.create({
  xtype: 'toolbar',
  docked: "top"
});
Ext.create({
  xtype: 'searchfield',
  ui: "faded",
  ref: function (field) {
    return _this.query = field;
  },
  placeholder: "Search...",
  onChange: this.onSearch.bind(this),
  responsiveConfig: (_a = {}, _a[small] = {
    flex: 1
  }, _a[medium] = {
    flex: undefined
  }, _a)
});
Ext.create({
  xtype: 'column',
  text: "Name",
  dataIndex: "name",
  flex: 2,
  resizable: true
});
Ext.create({
  xtype: 'column',
  text: "Email",
  dataIndex: "email",
  flex: 3,
  resizable: true,
  responsiveConfig: (_b = {}, _b[small] = {
    hidden: true
  }, _b[medium] = {
    hidden: false
  }, _b)
});
Ext.create({
  xtype: 'column',
  text: "Phone",
  dataIndex: "phone",
  flex: 2,
  resizable: true
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"searchfield"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'titlebar',
  title: "test",
  docked: "top"
});
Ext.create({
  xtype: 'button',
  align: "left",
  iconCls: "x-fa fa-bars",
  handler: this.toggleAppMenu,
  ripple: false
});
Ext.create({
  xtype: 'sheet',
  displayed: showAppMenu,
  side: "left",
  onHide: this.onHideAppMenu
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  title: "ExtReact Boilerplate"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"sheet"});
Ext.create({"xtype":"panel"});
Ext.require('Ext.data.TreeStore');
Ext.create({"xtype":"treelist"});
Ext.create({"xtype":"treelist"});
Ext.create('Ext.pivot.matrix.Local', {
  store: store,
  leftAxis: this.getLeftAxisConfig(),
  topAxis: this.getTopAxisConfig(),
  aggregate: [{
    dataIndex: 'costElementValue',
    header: 'Installation Base',
    aggregator: this.aggregateInstalationBase.bind(this),
    renderer: this.aggregateRenderer.bind(this)
  }]
});
Ext.create('Ext.data.Store', {
  fields: ['serviceOfferedGroup', 'warrantyGroup', 'costBlock', 'costElement', 'costElementValue'],
  data: data
});
Ext.create('Ext.pivot.plugin.Configurator', {
  panelWrap: false,
  fields: [{
    dataIndex: 'serviceOfferedGroup',
    header: 'Service Offered Group'
  }, {
    dataIndex: 'warrantyGroup',
    header: 'Warranty Group'
  }, {
    dataIndex: 'costBlock',
    header: 'Cost Block'
  }, {
    dataIndex: 'costElement',
    header: 'Cost Element'
  }]
});
Ext.create('Ext.pivot.plugin.Exporter', {});
Ext.create('Ext.pivot.plugin.DrillDown', {});
Ext.create('Ext.pivot.plugin.RangeEditor', {});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'pivotgrid',
  ref: function (pivotgrid) {
    return _this.pivotgrid = pivotgrid;
  },
  matrix: this.matrix,
  store: null,
  plugins: [this.getConfigurablePlugin(), this.getExportPlugin(), this.getDrillDownPlugin()],
  selectable: {
    mode: 'multi',
    drag: true,
    columns: true,
    cells: true,
    rows: true,
    extensible: true
  },
  onSelectionChange: this.onSelectionChange.bind(this)
});
Ext.create({
  xtype: 'toolbar',
  docked: "top",
  layout: {
    type: 'hbox',
    align: 'stretch'
  }
});
Ext.create({
  xtype: 'button',
  text: "Configurator",
  handler: function () {
    return _this.pivotgrid.showConfigurator();
  }
});
Ext.create({
  xtype: 'button',
  text: "Export to ..."
});
Ext.create({
  xtype: 'menu',
  defaults: {
    handler: function (menuItem) {
      return _this.export(menuItem);
    }
  }
});
Ext.create({
  xtype: 'menuitem',
  text: "Excel",
  cfg: {
    type: 'excel07',
    fileName: 'Pivot.xlsx',
    onlyExpandedNodes: true
  }
});
Ext.create({
  xtype: 'button',
  text: "Edit",
  handler: function () {
    return _this.onEditClick();
  }
});
Ext.create({"xtype":"pivotgrid"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"menu"});
Ext.create({"xtype":"menuitem"});
Ext.create({"xtype":"container"})