import { Container, TreeList } from "@extjs/ext-react";
import * as React from "react";
import { buildComponentUrl } from "../Common/Services/Ajax";

export class ReportListView extends React.Component<any, any> {

    private store = Ext.create('Ext.data.TreeStore', {
        "root": {
            "expanded": true,
            "children": [
                {
                    "text": "<a href='#' data-href='/report/Locap'>1. LOCAP reports (for a specific country)</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/Locap-Detailed'>2. LOCAP reports detailed</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/Contract'>3. Contract reports</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/ProActive-reports'>4. ProActive reports</a>",
                    "leaf": true
                },
                {
                    "text": "5. HDD retention reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-country'>a) on country level</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-central'>b) as central report</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-parameter'>c) HDD retention parameter</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "6. Calculation Parameter Overview reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-hw'>a) for HW maintenance cost elements (approved)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-hw-not-approved'>a) for HW maintenance cost elements (not approved)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-proactive'>b) for ProActive cost elements</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "7. New vs. old reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/CalcOutput-vs-FREEZE'>a) Country_CalcOutput actual vs. FREEZE report (e.g. Germany_CalcOutput actual vs. FREEZE)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/CalcOutput-new-vs-old'>b) Country_CalcOutput new vs. old report (e.g.Germany_CalcOutput new vs. old)</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "8. Solution Pack reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-ProActive-Costing'>a) Country SolutionPack ProActive Costing report (e.g. Germany_SolutionPack ProActive Costing)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-Price-List'>b) SolutionPack Price List report</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-Price-List-Details'>c) SolutionPack Price List Detailed report</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "<a href='#' data-href='/report/PO-Standard-Warranty-Material'>9. PO Standard Warranty Material Report</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/FLAT-Fee-Reports'>10. Availability fee report</a>",
                    "leaf": true
                },
                {
                    "text": "11. Software reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/SW-Service-Price-List'>a) Software services price list</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SW-Service-Price-List-detailed'>b) Software services price list detailed</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "12. Logistics reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "a) Logistics cost report",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-country'>1. local per country</a>",
                                    "leaf": true
                                },
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-central'>2. central with all country values</a>",
                                    "leaf": true
                                }
                            ]
                        },
                        {
                            "text": "b) Logistics cost report input currency",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-input-country'>1. local per country</a>",
                                    "leaf": true
                                },
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-input-central'>2. central with all country values</a>",
                                    "leaf": true
                                }
                            ]
                        },
                        {
                            "text": "c) Calculated logistics cost",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-calc-country'>1. local per country</a>",
                                    "leaf": true
                                },
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-calc-central'>2. central with all country values</a>",
                                    "leaf": true
                                }
                            ]
                        }
                    ]
                },
                {
                    "text": "13. LOCAP Global Support Packs(for all the countries)",
                    "leaf": true
                },
                {
                    "text": "14. Output Warranty t",
                    "leaf": true
                },
                {
                    "text": "15. AFR overview",
                    "leaf": true
                }
            ]
        }
    });

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container padding="25px" scrollable={true}>
                <div onClick={this.onOpenLink}>
                    <TreeList ref="treelist" store={this.store} width="100%" />
                </div>
            </Container>
        );
    }

    private init() {
        this.onOpenLink = this.onOpenLink.bind(this);
    }

    private onOpenLink(e) {

        let target = e.target as HTMLElement;
        let href = target.getAttribute('data-href');

        if (href) {
            href = buildComponentUrl(href);
            this.props.history.push(href);
        }
    }

}