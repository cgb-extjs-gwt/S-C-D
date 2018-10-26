import * as React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter as Router } from 'react-router-dom';
import { LayoutContainer } from './Layout/Components/LayoutContainer';
import { storeFactory } from './StoreFactory';
import { notifyWorker } from './Webworker/Notify';

declare var Ext: any;

// Enable responsiveConfig app-wide. You can remove this if you don't plan to build a responsive UI.
Ext.require('Ext.plugin.Responsive');

const store = storeFactory();

const notify = notifyWorker();
notify.onmessage = function (e: MessageEvent) {
    console.log(e.data);
};

/**
 * The main application view
 */
const App = () => {
    return (
        <Provider store={store}>
            <Router>
                <LayoutContainer />
            </Router>
        </Provider>
    )
}

export default App;
