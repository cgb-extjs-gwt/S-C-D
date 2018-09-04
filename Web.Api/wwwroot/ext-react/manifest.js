Ext.require(["Ext.app.Application", "Ext.Component", "Ext.Widget"]);
Ext.require("Ext.reactor.RendererCell");
Ext.create('Ext.data.Store', {
  pageSize: 50,
  fields: ['countryName', 'countryId', 'reactionTimeName', 'reactionTimeId', 'reactionTypeName', 'reactionTypeId', 'serviceLocatorName', 'serviceLocatorId', 'isApplicable', 'innerId'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/AvailabilityFeeAdmin/GetAll',
      update: 'api/AvailabilityFeeAdmin/SaveAll'
    },
    reader: {
      type: 'json',
      rootProperty: 'items',
      totalProperty: 'total'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {}
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Availability Fee Settings',
  store: this.store,
  cls: "filter-grid",
  columnLines: true,
  plugins: ['pagingtoolbar']
});
Ext.create({
  xtype: 'column',
  text: "Country",
  dataIndex: "countryName",
  flex: 1
});
Ext.create({
  xtype: 'column',
  text: "Reaction Time",
  dataIndex: "reactionTimeName",
  flex: 1
});
Ext.create({
  xtype: 'column',
  text: "Reaction Type",
  dataIndex: "reactionTypeName",
  flex: 1
});
Ext.create({
  xtype: 'column',
  text: "Service Locator",
  dataIndex: "serviceLocatorName",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Is Applicable",
  dataIndex: "isApplicable",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settings',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settingsss',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settings',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settingsss',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settings',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settingssss',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settings',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
  autoLoad: true,
  proxy: {
    type: 'ajax',
    api: {
      read: 'api/Country/GetAll',
      update: 'api/Country/SaveAll'
    },
    reader: {
      type: 'json',
      idProperty: 'id'
    },
    writer: {
      type: 'json',
      writeAllFields: true,
      idProperty: 'id',
      allowSingle: false
    },
    listeners: {
      exception: function (proxy, response, operation) {
        console.log(operation.getError());
      }
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecords = _this.store.getUpdatedRecords();
      if (modifiedRecords.length > 0) {
        _this.setState({
          disableSaveButton: false
        });
      } else {
        _this.setState({
          disableSaveButton: true
        });
      }
    }
  }
});
Ext.create({
  xtype: 'grid',
  title: 'Country Settingsss',
  store: this.store,
  cls: "filter-grid",
  columnLines: true
});
Ext.create({
  xtype: 'column',
  text: "Country Name",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override List and Dealer Price",
  dataIndex: "canOverrideListAndDealerPrices",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Show Dealer Price",
  dataIndex: "showDealerPrice",
  flex: 1
});
Ext.create({
  xtype: 'checkcolumn',
  text: "Can Override TC and TP",
  dataIndex: "canOverrideTransferCostAndPrice",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  handler: this.saveRecords,
  iconCls: "x-fa fa-save",
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.require(['Ext.grid.plugin.Editable', 'Ext.grid.plugin.CellEditing']);
Ext.define('RoleCode', {
  extend: 'Ext.data.Model',
  fields: [{
    name: 'id',
    type: 'int'
  }, {
    name: 'name',
    type: 'string'
  }],
  belongsTo: 'WarrantyGroup'
});
Ext.create('Ext.data.Store', {
  model: 'RoleCode',
  autoLoad: true,
  pageSize: 0,
  proxy: {
    type: 'ajax',
    writer: {
      type: 'json',
      writeAllFields: true,
      allowSingle: false,
      idProperty: "id"
    },
    reader: {
      type: 'json',
      idProperty: "id"
    },
    listeners: {
      exception: function (proxy, response, operation) {}
    },
    api: {
      create: 'api/rolecode/SaveAll',
      read: 'api/rolecode/GetAll',
      update: 'api/rolecode/SaveAll',
      destroy: 'api/rolecode/DeleteAll'
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecordsCount = _this.store.getUpdatedRecords().length;
      _this.saveButtonHandler(modifiedRecordsCount);
    },
    datachanged: function (store) {
      var modifiedRecordsCount = _this.store.getModifiedRecords().length + _this.store.getRemovedRecords().length;
      _this.saveButtonHandler(modifiedRecordsCount);
    }
  }
});
Ext.create('RoleCode', {
  id: 0,
  name: 'new'
});
Ext.create({
  xtype: 'grid',
  title: 'Role codes',
  store: this.store,
  cls: "filter-grid",
  columnLines: true,
  shadow: true,
  onSelectionChange: function (dataView, records, selected, selection) {
    return _this.selectRowHandler(dataView, records, selected, selection);
  },
  platformConfig: {
    desktop: {
      plugins: {
        gridcellediting: true
      }
    },
    '!desktop': {
      plugins: {
        grideditable: true
      }
    }
  }
});
Ext.create({
  xtype: 'column',
  text: "ID",
  dataIndex: "id",
  width: "80"
});
Ext.create({
  xtype: 'column',
  text: "Role code",
  flex: 1,
  dataIndex: "name",
  editable: true
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "New",
  flex: 1,
  iconCls: "x-fa fa-plus",
  handler: this.newRecord,
  disabled: this.state.disableNewButton
});
Ext.create({
  xtype: 'button',
  text: "Delete",
  flex: 1,
  iconCls: "x-fa fa-trash",
  handler: this.deleteRecord,
  disabled: this.state.disableDeleteButton
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  iconCls: "x-fa fa-save",
  handler: this.saveRecords,
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.define('UserRole', {
  extend: 'Ext.data.Model',
  fields: ['id', 'userId', 'countryId', 'roleId']
});
Ext.create('Ext.data.Store', {
  model: 'UserRole',
  autoLoad: true,
  pageSize: 0,
  proxy: {
    type: 'ajax',
    writer: {
      type: 'json',
      writeAllFields: true,
      allowSingle: false,
      idProperty: "id"
    },
    reader: {
      type: 'json',
      idProperty: "id"
    },
    api: {
      create: 'api/userrole/SaveAll',
      read: 'api/userrole/GetAll',
      update: 'api/userrole/SaveAll',
      destroy: 'api/userrole/DeleteAll'
    }
  }
});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name'],
  autoLoad: false,
  pageSize: 0,
  sorters: [{
    property: 'name',
    direction: 'ASC'
  }],
  proxy: {
    type: 'ajax',
    reader: {
      type: 'json'
    },
    api: {
      read: 'api/User/GetAll'
    }
  },
  listeners: {
    datachanged: function (store) {
      _this.setState({
        storeUserReady: true
      });
    }
  }
});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name'],
  autoLoad: false,
  pageSize: 0,
  sorters: [{
    property: 'name',
    direction: 'ASC'
  }],
  proxy: {
    type: 'ajax',
    reader: {
      type: 'json'
    },
    api: {
      read: 'api/Country/GetAll'
    }
  },
  listeners: {
    datachanged: function (store) {
      _this.setState({
        storeCountryReady: true
      });
    }
  }
});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name'],
  autoLoad: false,
  pageSize: 0,
  sorters: [{
    property: 'name',
    direction: 'ASC'
  }],
  proxy: {
    type: 'ajax',
    reader: {
      type: 'json'
    },
    api: {
      read: 'api/Role/GetAll'
    }
  },
  listeners: {
    datachanged: function (store) {
      _this.setState({
        storeRoleReady: true
      });
    }
  }
});
Ext.create({
  xtype: 'container',
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create('UserRole', {
  userId: _this.userComboBox.getValue(),
  roleId: _this.roleComboBox.getValue(),
  countryId: _this.countryComboBox.getValue()
});
Ext.create({
  xtype: 'dialog',
  displayed: isVisibleForm,
  closable: true,
  closeAction: "hide",
  ref: function (form) {
    return _this.userRoleForm = form;
  },
  platformConfig: {
    "!phone": {
      maxHeight: 500,
      width: 350
    }
  },
  onHide: this.onFormCancel
});
Ext.create({
  xtype: 'comboboxfield',
  ref: function (combobox) {
    return _this.userComboBox = combobox;
  },
  store: storeUser,
  valueField: "id",
  displayField: "name",
  label: "User",
  queryMode: "local",
  value: selectedRecord && selectedRecord.data.userId,
  editable: false,
  required: true,
  onChange: this.isValidInput.bind(this)
});
Ext.create({
  xtype: 'comboboxfield',
  store: storeRole,
  valueField: "id",
  displayField: "name",
  label: "Role",
  queryMode: "local",
  value: selectedRecord && selectedRecord.data.roleId,
  ref: function (combobox) {
    return _this.roleComboBox = combobox;
  },
  editable: false,
  required: true,
  onChange: this.onRoleChange
});
Ext.create({
  xtype: 'comboboxfield',
  ref: function (combobox) {
    return _this.countryComboBox = combobox;
  },
  store: storeCountry,
  valueField: "id",
  displayField: "name",
  label: "Country",
  queryMode: "local",
  value: selectedRecord && selectedRecord.data.countryId,
  editable: false,
  required: true,
  hidden: this.state.countryFieldHidden,
  onChange: this.isValidInput.bind(this)
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Save",
  handler: this.onFormSave.bind(this),
  flex: 1,
  disabled: !this.state.isValid
});
Ext.create({
  xtype: 'button',
  text: "Cancel",
  handler: this.onFormCancel,
  flex: 1
});
Ext.create({"xtype":"comboboxfield"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"dialog"});
Ext.create({
  xtype: 'column',
  text: "User",
  dataIndex: "userId",
  flex: 1,
  renderer: renderer.bind(this)
});
Ext.create({
  xtype: 'column',
  text: "Country",
  dataIndex: "countryId",
  flex: 1,
  renderer: renderer.bind(this)
});
Ext.create({
  xtype: 'column',
  text: "Role",
  dataIndex: "roleId",
  flex: 1,
  renderer: renderer.bind(this)
});
Ext.create({
  xtype: 'grid',
  title: 'User roles',
  store: store,
  cls: "filter-grid",
  columnLines: true,
  shadow: true,
  emptyText: " "
});
Ext.create({
  xtype: 'column',
  text: "Actions",
  flex: 1,
  dataIndex: ""
});
Ext.create({
  xtype: 'gridcell',
  tools: {
    gear: {
      tooltip: "Edit",
      handler: this.onEditButtonClick
    },
    close: {
      tooltip: "Delete",
      handler: this.onDeleteButtonClick
    }
  },
  value: ""
});
Ext.create({
  xtype: 'toolbar',
  docked: "top"
});
Ext.create({
  xtype: 'button',
  text: "New",
  iconCls: "x-fa fa-plus",
  handler: this.onNewButtonClick,
  width: "100",
  textAlign: "left"
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"gridcell"});
Ext.require(['Ext.grid.plugin.Editable', 'Ext.grid.plugin.CellEditing']);
Ext.create('Ext.data.Store', {
  fields: ['id', 'name', {
    name: 'roleCodeId',
    type: 'int',
    convert: function (val, row) {
      if (!val) return '';
      return val;
    }
  }, {
    name: 'roleCodeEmpty',
    type: 'bool',
    convert: function (val, row) {
      if (row.data.roleCodeId == 0) return false;
      return true;
    }
  }],
  autoLoad: true,
  pageSize: 0,
  proxy: {
    type: 'ajax',
    writer: {
      type: 'json',
      writeAllFields: true,
      allowSingle: false,
      idProperty: "id"
    },
    reader: {
      type: 'json',
      idProperty: "id"
    },
    api: {
      read: 'api/WarrantyGroup/GetAll',
      update: 'api/WarrantyGroup/SaveAll'
    }
  },
  listeners: {
    update: function (store, record, operation, modifiedFieldNames, details, eOpts) {
      var modifiedRecordsCount = _this.store.getUpdatedRecords().length;
      _this.saveButtonHandler(modifiedRecordsCount);
    }
  }
});
Ext.create('Ext.data.Store', {
  fields: ['id', 'name'],
  autoLoad: false,
  pageSize: 0,
  sorters: [{
    property: 'Name',
    direction: 'ASC'
  }],
  proxy: {
    type: 'ajax',
    reader: {
      type: 'json'
    },
    api: {
      read: 'api/RoleCode/GetAll'
    }
  },
  listeners: {
    datachanged: function (store) {
      _this.setState({
        render: true
      });
    }
  }
});
Ext.create({
  xtype: 'comboboxfield',
  store: this.storeRoleCode,
  valueField: "id",
  displayField: "name",
  label: "Select role code",
  queryMode: "local"
});
Ext.create({
  xtype: 'column',
  text: "Role code",
  dataIndex: "roleCodeId",
  flex: 1,
  editable: true,
  renderer: renderer.bind(this)
});
Ext.create({
  xtype: 'grid',
  title: 'Warranty groups',
  store: this.store,
  cls: "filter-grid",
  columnLines: true,
  shadow: true,
  onSelectionChange: function (dataView, records, selected, selection) {
    return _this.selectRowHandler(dataView, records, selected, selection);
  },
  platformConfig: {
    desktop: {
      plugins: {
        gridcellediting: true
      }
    },
    '!desktop': {
      plugins: {
        grideditable: true
      }
    }
  }
});
Ext.create({
  xtype: 'column',
  text: "WG",
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "top"
});
Ext.create({
  xtype: 'checkboxfield',
  boxLabel: "Show only WGs with no Role code",
  onChange: function (chkBox, newValue, oldValue) {
    return _this.filterOnChange(chkBox, newValue, oldValue);
  }
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Manage role codes",
  flex: 1,
  iconCls: "x-fa fa-users",
  handler: this.ManageRoleCodes
});
Ext.create({
  xtype: 'button',
  text: "Cancel",
  flex: 1,
  iconCls: "x-fa fa-trash",
  handler: this.cancelChanges,
  disabled: this.state.disableCancelButton
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  iconCls: "x-fa fa-save",
  handler: this.saveRecords,
  disabled: this.state.disableSaveButton
});
Ext.create({"xtype":"comboboxfield"});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"checkboxfield"});
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.require('Ext.plugin.Responsive');
Ext.create({
  xtype: 'container',
  layout: "vbox",
  padding: "10px"
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "country",
  width: "250px",
  label: "Country:",
  labelAlign: "left",
  labelWidth: "80px",
  options: this.state.countries,
  displayField: "name",
  valueField: "id",
  queryMode: "local",
  clearable: "true",
  onChange: this.onCountryChange
});
Ext.create({
  xtype: 'container',
  layout: "hbox"
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'vbox',
    align: 'left'
  },
  defaults: {
    disabled: !this.state.isPortfolio
  },
  margin: "15px 0"
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "globPort",
  boxLabel: "Fujitsu global portfolio"
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "masterPort",
  boxLabel: "Master portfolio"
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "corePort",
  boxLabel: "Core portfolio"
});
Ext.create({"xtype":"container"});
Ext.create({
  xtype: 'button',
  text: "Deny combinations",
  ui: "decline",
  padding: "0 10px 0 0",
  handler: this.onDeny
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"checkboxfield"});
Ext.create({"xtype":"comboboxfield"});
Ext.create({
  xtype: 'container',
  scrollable: true
});
Ext.create({
  xtype: 'toolbar',
  docked: "top"
});
Ext.create({
  xtype: 'button',
  iconCls: "x-fa fa-edit",
  text: "Edit",
  handler: this.onEdit
});
Ext.create({
  xtype: 'button',
  iconCls: "x-fa fa-undo",
  text: "Allow combinations",
  ui: "confirm",
  handler: this.onAllow
});
Ext.create({
  xtype: 'grid',
  ref: "denied",
  store: this.state.denied,
  width: "100%",
  minHeight: "45%",
  title: "Denied combinations",
  selectable: "multi",
  plugins: ['pagingtoolbar']
});
Ext.create({
  xtype: 'grid',
  ref: "allowed",
  store: this.state.allowed,
  width: "100%",
  minHeight: "45%",
  title: "Allowed combinations",
  selectable: false,
  plugins: ['pagingtoolbar']
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  margin: "10px 0",
  defaults: {
    maxWidth: '200px',
    valueField: 'id',
    displayField: 'name',
    queryMode: 'local',
    clearable: 'true'
  }
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "country",
  label: "Country:",
  options: this.state.countries,
  onChange: this.onCountryChange
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "wg",
  label: "Asset(WG):",
  options: this.state.warrantyGroups
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "availability",
  label: "Availability:",
  options: this.state.availabilityTypes
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "duration",
  label: "Duration:",
  options: this.state.durationTypes
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "reactType",
  label: "Reaction type:",
  options: this.state.reactTypes
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "reactTime",
  label: "Reaction time:",
  options: this.state.reactionTimeTypes
});
Ext.create({
  xtype: 'comboboxfield',
  ref: "srvLoc",
  label: "Service location:",
  options: this.state.serviceLocationTypes
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'vbox',
    align: 'left'
  },
  defaults: {
    disabled: !this.state.isPortfolio,
    padding: '3px 0'
  }
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "globPort",
  boxLabel: "Fujitsu global portfolio"
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "masterPort",
  boxLabel: "Master portfolio"
});
Ext.create({
  xtype: 'checkboxfield',
  ref: "corePort",
  boxLabel: "Core portfolio"
});
Ext.create({
  xtype: 'button',
  text: "Search",
  ui: "action",
  width: "85px",
  handler: this.onSearch,
  margin: "20px auto"
});
Ext.create({"xtype":"comboboxfield"});
Ext.create({"xtype":"checkboxfield"});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  width: width,
  maxWidth: maxWidth
});
Ext.create({
  xtype: 'label',
  html: title,
  padding: "7px"
});
Ext.create({
  xtype: 'list',
  ref: "lst",
  itemTpl: itemTpl,
  store: store,
  height: height,
  maxHeight: maxHeight,
  selectable: selectable,
  scrollable: "true"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"label"});
Ext.create({"xtype":"list"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"checkcolumn"});
Ext.require('Ext.panel.Collapser');
Ext.create({
  xtype: 'panel',
  title: this.getTitle(),
  layout: "fit",
  shadow: true,
  collapsed: true,
  collapsible: {
    direction: 'top',
    dynamic: true
  },
  onExpand: this.onPanelExpanded,
  margin: "5px 10px 5px 10px"
});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  layout: "vbox",
  flex: flex,
  scrollable: true
});
Ext.create({
  xtype: 'container',
  layout: "center",
  padding: "20"
});
Ext.create({"xtype":"container"});
Ext.create({
  xtype: 'container',
  layout: {
    type: "hbox",
    pack: "space-between"
  }
});
Ext.create({"xtype":"container"});
Ext.create('Ext.data.Store', {
  fields: columns.map(function (column) {
    return {
      name: column.dataIndex
    };
  }),
  autoLoad: true,
  proxy: {
    type: 'ajax',
    url: dataLoadUrl,
    reader: {
      type: 'json'
    }
  }
});
Ext.create({
  xtype: 'container',
  layout: "vbox"
});
Ext.create({
  xtype: 'grid',
  store: this.store,
  columnLines: true,
  height: 400
});
Ext.create({
  xtype: 'column',
  key: id + "_" + column.dataIndex,
  text: column.title,
  dataIndex: column.dataIndex,
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Approve",
  handler: this.onApprove,
  flex: 1
});
Ext.create({
  xtype: 'button',
  text: "Reject",
  handler: this.onReject,
  flex: 1
});
Ext.create({
  xtype: 'container',
  layout: "hbox"
});
Ext.create({
  xtype: 'formpanel',
  defaults: {
    labelAlign: 'left'
  },
  flex: 1,
  ref: function (form) {
    return _this.rejectForm = form;
  }
});
Ext.create({
  xtype: 'textfield',
  ref: function (textField) {
    return _this.rejectMessageTextField = textField;
  },
  required: true,
  placeholder: "Please enter the reason for rejection",
  onChange: this.onRejectMessageTextFieldChange
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Send back to requestor",
  handler: this.onSendBackToRequestor,
  flex: 1,
  disabled: !isValidRejectForm
});
Ext.create({
  xtype: 'button',
  text: "Cancel",
  handler: this.onRejectCancel,
  flex: 1
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"textfield"});
Ext.create({"xtype":"formpanel"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({
  xtype: 'radiofield',
  name: "application",
  key: item.id,
  boxLabel: item.name,
  value: item.id,
  checked: item.id === _this.props.application.selectedItemId,
  onCheck: function () {
    return _this.props.onApplicationSelect(item.id);
  }
});
Ext.create({
  xtype: 'checkboxfield',
  name: "costBlockIds",
  key: item.id,
  boxLabel: item.name,
  value: item.id,
  checked: _this.props.costBlocks.selectedItemIds.indexOf(item.id) > -1,
  onCheck: function () {
    return _this.props.onCostBlockCheck(item.id);
  },
  onUnCheck: function () {
    return _this.props.onCostBlockUncheck(item.id);
  }
});
Ext.create({
  xtype: 'checkboxfield',
  name: "costElementIds",
  key: item.element.id,
  boxLabel: item.element.name,
  value: item.element.id,
  checked: _this.props.costElements.selectedItemIds.indexOf(item.element.id) > -1,
  onCheck: function () {
    return _this.props.onCostElementCheck(item.element.id, item.parentId);
  },
  onUnCheck: function () {
    return _this.props.onCostElementUncheck(item.element.id);
  }
});
Ext.create({
  xtype: 'formpanel',
  scrollable: true,
  shadow: true,
  layout: {
    type: 'vbox',
    align: 'left'
  },
  flex: 1,
  title: "Filter By"
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'hbox'
  }
});
Ext.create({
  xtype: 'panel',
  layout: {
    type: 'vbox',
    align: 'left'
  }
});
Ext.create({
  xtype: 'panel',
  margin: '0 15px',
  layout: {
    type: 'vbox',
    align: 'left'
  }
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'hbox'
  }
});
Ext.create({
  xtype: 'panel',
  layout: {
    type: 'vbox',
    align: 'left'
  }
});
Ext.create({
  xtype: 'panel',
  margin: '0 15px',
  layout: {
    type: 'vbox',
    align: 'left'
  }
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'hbox'
  }
});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'datepickerfield',
  value: this.props.startDate,
  destroyPickerOnHide: true,
  dateFormat: "d.m.Y",
  label: "From",
  picker: {
    yearFrom: 2018
  },
  onChange: function (el, newDate) {
    return _this.props.onStartDateChange(newDate);
  }
});
Ext.create({
  xtype: 'panel',
  margin: '0 20px'
});
Ext.create({
  xtype: 'datepickerfield',
  value: this.props.endDate,
  destroyPickerOnHide: true,
  label: "To",
  dateFormat: "d.m.Y",
  picker: {
    yearFrom: 2018
  },
  onChange: function (el, newDate) {
    return _this.props.onEndDateChange(newDate);
  }
});
Ext.create({
  xtype: 'container',
  margin: "20px 0 0 0",
  layout: {
    type: 'hbox',
    align: 'center'
  }
});
Ext.create({
  xtype: 'button',
  disabled: costElementsCheckBoxes ? false : true,
  iconCls: "x-fa fa-filter",
  text: "Filter",
  ui: "action raised",
  handler: this.props.onApplyFilter
});
Ext.create({
  xtype: 'container',
  masked: {
    xtype: "loadmask",
    message: "Loading"
  }
});
Ext.create({"xtype":"formpanel"});
Ext.create({"xtype":"radiofield"});
Ext.create({"xtype":"checkboxfield"});
Ext.create({"xtype":"panel"});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"datepickerfield"});
Ext.create({"xtype":"button"});
Ext.require(['Ext.grid.plugin.CellEditing']);
Ext.create('Ext.data.Store', {
  data: region.selectedList ? region.selectedList.list : []
});
Ext.create({
  xtype: 'container',
  layout: {
    type: 'hbox',
    align: 'stretch '
  }
});
Ext.create({
  xtype: 'container',
  flex: 1,
  layout: "vbox",
  shadow: true
});
Ext.create({
  xtype: 'container',
  layout: "hbox",
  flex: 3
});
Ext.create({
  xtype: 'formpanel',
  flex: 1
});
Ext.create({
  xtype: 'panel',
  title: "Description",
  padding: "10",
  scrollable: true,
  flex: 1
});
Ext.create({
  xtype: 'label',
  html: costElement.description
});
Ext.create({
  xtype: 'container',
  flex: 1,
  layout: "vbox",
  padding: "0px 0px 0px 5px"
});
Ext.create({
  xtype: 'container',
  layout: "hbox",
  flex: 1
});
Ext.create({
  xtype: 'formpanel',
  flex: 1
});
Ext.create({
  xtype: 'comboboxfield',
  label: region.name,
  displayField: "name",
  valueField: "id",
  queryMode: "local",
  store: regionStore,
  selection: selectedRegion,
  onChange: function (combobox, newValue, oldValue) {
    return _this.props.onRegionSelected(newValue);
  }
});
Ext.create({
  xtype: 'radiofield',
  key: name + "_" + item.id,
  boxLabel: item.name,
  name: name,
  checked: item.id === selectedCostElementId,
  disabled: !isEnabled,
  onCheck: function (radioField) {
    if (radioField.hasFocus) {
      onSelected(item);
    }

    return false;
  }
});
Ext.create({
  xtype: 'containerfield',
  label: label,
  layout: {
    type: 'vbox',
    align: 'left'
  },
  scrollable: true,
  maxHeight: "500px"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"comboboxfield"});
Ext.create({"xtype":"panel"});
Ext.create({"xtype":"formpanel"});
Ext.create({"xtype":"radiofield"});
Ext.create({"xtype":"containerfield"});
Ext.create({"xtype":"label"});
Ext.require('Ext.MessageBox');
Ext.create('Ext.data.Store', {
  data: application && application.list
});
Ext.create({
  xtype: 'container',
  layout: "vbox"
});
Ext.create({
  xtype: 'formpanel',
  defaults: {
    labelAlign: 'left'
  }
});
Ext.create({
  xtype: 'tabpanel',
  key: application.selectedItemId,
  flex: 1,
  tabBar: {
    layout: {
      pack: 'left'
    }
  },
  activeTab: costBlocks.list.findIndex(function (costBlock) {
    return costBlock.id === costBlocks.selectedItemId;
  }),
  onActiveItemChange: function (tabPanel, newValue, oldValue) {
    return _this.onActiveTabChange(tabPanel, newValue, oldValue);
  }
});
Ext.create({
  xtype: 'comboboxfield',
  label: "Application",
  width: "300",
  displayField: "name",
  valueField: "id",
  queryMode: "local",
  store: applicatonStore,
  selection: selectedApplication,
  onChange: function (combobox, newValue, oldValue) {
    return onApplicationSelected && onApplicationSelected(newValue);
  }
});
Ext.create({
  xtype: 'container',
  key: costBlockTab.id,
  title: costBlockTab.name,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"formpanel"});
Ext.create({"xtype":"comboboxfield"});
Ext.create({"xtype":"tabpanel"});
Ext.create('Ext.data.Store', {
  data: Array.from(this.itemsMap.values()),
  listeners: onItemEdited && ({
    update: function (store, record, operation, modifiedFieldNames, details) {
      if (modifiedFieldNames[0] === 'name') {
        record.reject();
      } else {
        var item = record.data;
        me.itemsMap.set(item.id, item);
        record.set('valueCount', 1);
        onItemEdited(record.data);
      }
    }
  })
});
Ext.create({
  xtype: 'grid',
  flex: 1,
  store: store,
  shadow: true,
  columnLines: true,
  plugins: ['cellediting', 'selectionreplicator'],
  selectable: {
    rows: true,
    cells: true,
    columns: true,
    drag: true,
    extensible: 'y'
  },
  onSelectionchange: this.onSelected
});
Ext.create({
  xtype: 'column',
  text: nameColumnTitle,
  dataIndex: "name",
  flex: 1,
  extensible: false
});
Ext.create({"xtype":"column"});
Ext.create({
  xtype: 'selectfield',
  options: columProps.selectedItems.map(function (item) {
    return {
      text: item.name,
      value: item.id
    };
  })
});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"column"});
Ext.create({
  xtype: 'selectfield',
  options: [{
    text: 'true',
    value: 1
  }, {
    text: 'false',
    value: 0
  }]
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"selectfield"});
Ext.create({"xtype":"column"});
Ext.require(['Ext.grid.plugin.CellEditing', 'Ext.panel.Resizer']);
Ext.create({
  xtype: 'container',
  layout: "vbox",
  flex: props.flex
});
Ext.create({
  xtype: 'toolbar',
  docked: "top"
});
Ext.create({
  xtype: 'button',
  text: "Apply filters",
  flex: 1,
  disabled: !props.isEnableApplyFilters,
  handler: props.onApplyFilters
});
Ext.create({
  xtype: 'button',
  text: "History",
  flex: 1,
  disabled: this.state.selectedItems.length != 1,
  handler: this.showHistoryWindow
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Cancel",
  flex: 1,
  disabled: !props.isEnableClear,
  handler: function () {
    return _this.showClearDialog();
  }
});
Ext.create({
  xtype: 'button',
  text: "Save",
  flex: 1,
  disabled: !props.isEnableSave,
  handler: function () {
    return _this.showSaveDialog(false);
  }
});
Ext.create({
  xtype: 'button',
  text: "Save and send for approval",
  flex: 1,
  disabled: !props.isEnableSave,
  handler: function () {
    return _this.showSaveDialog(true);
  }
});
Ext.create({
  xtype: 'dialog',
  displayed: isVisibleHistoryWindow,
  title: "History",
  closable: true,
  maximizable: true,
  resizable: {
    dynamic: true,
    edges: 'all'
  },
  minHeight: "600",
  minWidth: "700",
  onClose: this.closeHistoryWindow,
  layout: "fit"
});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create({"xtype":"dialog"});
Ext.create({"xtype":"container"});
Ext.create('Ext.data.Store', {
  data: items && items.slice(),
  listeners: {
    update: onSelectionChanged && (function (store, record, operation, modifiedFieldNames, details) {
      return onSelectionChanged(record.data, record.data.isChecked);
    })
  }
});
Ext.create({
  xtype: 'grid',
  title: title || '',
  store: store,
  flex: flex,
  shadow: true,
  cls: "filter-grid",
  height: height,
  columnLines: true
});
Ext.create({
  xtype: 'checkcolumn',
  width: "70",
  dataIndex: "isChecked"
});
Ext.create({
  xtype: 'column',
  text: valueColumnText || '',
  dataIndex: "name",
  flex: 1
});
Ext.create({
  xtype: 'toolbar',
  docked: "bottom"
});
Ext.create({
  xtype: 'button',
  text: "Reset",
  flex: 1,
  handler: function () {
    return onReset && onReset();
  },
  disabled: !items || items.findIndex(function (item) {
    return item.isChecked;
  }) === -1
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"checkcolumn"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"toolbar"});
Ext.create({"xtype":"button"});
Ext.create('Ext.data.Store', {
  fields: [{
    name: EDIT_DATE_COLUMN_NAME,
    type: 'date'
  }, {
    name: EDIT_USER_NAME_COLUMN_NAME,
    type: 'string'
  }, {
    name: VALUE_COLUMN_NAME
  }],
  autoLoad: true,
  remoteSort: true,
  pageSize: 100,
  proxy: {
    type: 'ajax',
    url: dataLoadUrl,
    reader: {
      type: 'json'
    }
  }
});
Ext.create({
  xtype: 'grid',
  store: this.store,
  columnLines: true,
  border: true,
  minHeight: "400"
});
Ext.create({
  xtype: 'datecolumn',
  dataIndex: EDIT_DATE_COLUMN_NAME,
  text: "Date",
  format: DateFormats.dateTime,
  flex: 1,
  groupable: false
});
Ext.create({
  xtype: 'column',
  dataIndex: EDIT_USER_NAME_COLUMN_NAME,
  text: "User",
  flex: 1,
  groupable: false
});
Ext.create({
  xtype: 'column',
  dataIndex: VALUE_COLUMN_NAME,
  text: "Value",
  flex: 1,
  groupable: false
});
Ext.create({"xtype":"grid"});
Ext.create({"xtype":"column"});
Ext.create({"xtype":"datecolumn"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.create({
  xtype: 'container',
  fullscreen: true,
  layout: "fit"
});
Ext.create({
  xtype: 'panel',
  scrollable: true,
  docked: "left",
  shadow: true,
  zIndex: 2
});
Ext.create({
  xtype: 'titlebar',
  title: "SCD 2.0",
  docked: "top"
});
Ext.create({
  xtype: 'panel',
  title: title,
  layout: "fit"
});
Ext.create({"xtype":"container"});
Ext.create({"xtype":"titlebar"});
Ext.create({"xtype":"panel"});
Ext.require('Ext.data.TreeStore');
Ext.create({"xtype":"treelist"});
Ext.create({"xtype":"treelist"});
Ext.create({
  xtype: 'container',
  padding: "20"
});
Ext.create({"xtype":"container"});
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