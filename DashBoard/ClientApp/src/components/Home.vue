<template>
    <div class="loading-parent" :style="loaded ? 'display: none' : 'display: flex'">
        <div>loading...</div>
    </div>
    <div :style="loaded ? 'display: block' : 'display: none'">
        <div class="row">
            <div class="card">
                <div class="row" style="padding: 2rem;">
                    <div id="chart" class="chart"></div>
                </div>
            </div>
            <div class="card"></div>
            <div class="card"></div>
        </div>
        <div class="row">
            <div class="card">
                <div class="occupance-graphs-section">
                    <div class="occupance-graph">
                        <template v-for="spot in first_floor">
                            <template v-if="spot == 1">
                                <div class="square taken" :key="spot"></div>
                            </template>
                            <template v-if="spot == 0">
                                <div class="square free" :key="spot"></div>
                            </template>
                        </template>
                    </div>
                    <div class="occupance-graph">
                        <template v-for="spot in second_floor">
                            <template v-if="spot == 1">
                                <div class="square taken" :key="spot"></div>
                            </template>
                            <template v-if="spot == 0">
                                <div class="square free" :key="spot"></div>
                            </template>
                        </template>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .row {
        flex-wrap: wrap;
    }

    .occupance-graphs-section {
        display: flex;
        flex-direction: row;
        justify-content: space-evenly;
        padding: 2rem;
        width: 100%;
    }

    .occupance-graph {
        display: flex;
        flex-wrap: wrap;
        align-content: center;
        width: 100%;
        max-width: 615px;
        margin: 0 1rem;
    }

    .square {
        display: flex;
        border-radius: 8px;
        width: 36px;
        height: 36px;
        margin: 4px;
    }

    .taken {
        background-color: #F7AA00
    }

    .taken:hover {
        background-color: #f79400
    }

    .free {
        background-color: #00F790
    }

    .free:hover {
        background-color: #00ac4d
    }

    .loading-parent {
        width: 100%;
        height: 100%;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    @media screen and (max-width: 1040px) {
        .occupance-graphs-section {
            flex-direction: column;
        }

        .occupance-graph {
            margin: 1rem 0;
            justify-content: center;
        }
    }
</style>

<script>
import ApexCharts from 'apexcharts'

export default {
    name: 'Home',
    data() {
        return {
            first_floor: {
                "001": 0,
                "002": 1,
                "003": 0,
                "004": 1,
                "005": 0,
                "006": 0,
                "007": 1,
                "008": 0,
                "009": 1,
                "010": 1,
                "011": 0,
                "012": 0,
                "013": 0,
                "014": 0,
                "015": 1,
                "016": 1,
                "017": 0,
                "018": 0,
                "019": 1,
                "020": 0,
                "021": 0,
                "022": 1,
                "023": 1,
                "024": 0,
                "025": 1,
                "026": 0,
                "027": 0,
                "028": 0,
                "029": 1,
                "030": 0,
                "031": 1,
                "032": 1,
                "033": 1,
                "034": 1,
                "035": 1,
                "036": 1,
                "037": 0,
                "038": 1,
                "039": 1,
                "040": 1,
                "041": 1,
                "042": 0,
                "043": 1,
                "044": 1,
                "045": 0,
                "046": 0,
                "047": 1,
                "048": 0,
                "049": 1,
                "050": 0,
            },
            second_floor: {
                "101": 0,
                "102": 1,
                "103": 0,
                "104": 0,
                "105": 0,
                "106": 0,
                "107": 0,
                "108": 1,
                "109": 1,
                "110": 1,
                "111": 0,
                "112": 1,
                "113": 1,
                "114": 1,
                "115": 0,
                "116": 0,
                "117": 0,
                "118": 0,
                "119": 0,
                "120": 1,
                "121": 0,
                "122": 0,
                "123": 1,
                "124": 1,
                "125": 0,
                "126": 0,
                "127": 0,
                "128": 1,
                "129": 1,
                "130": 0,
                "131": 0,
                "132": 0,
                "133": 0,
                "134": 0,
                "135": 1,
                "136": 1,
                "137": 0,
                "138": 0,
                "139": 0,
                "140": 0,
                "141": 0,
                "142": 0,
                "143": 1,
                "144": 1,
                "145": 0,
                "146": 0,
                "147": 1,
                "148": 0,
                "149": 1,
                "150": 1,
            },
            loaded: false
        }
    },
    mounted() {
        let chartTextColor = '#b9b9b9'
        let optionsLine = {
            chart: {
                height: "100%",
                width: "100%",
                foreColor: chartTextColor,
                toolbar: { show: false },
                type: 'line',
                zoom: { enabled: false },
                dropShadow: { enabled: false }
            },
            tooltip: { theme: "dark", style: { fontFamily: 'Inter' } },
            stroke: { curve: 'smooth', width: 2 },
            colors: ["#00F790", '#F7AA00'],
            series: [
                { name: "Produced", data: [2, 5, 7, 4, 2, 7] },
                { name: "Discarded", data: [5, 8, 9, 4, 9, 3] }
            ],
            markers: { size: 0, strokeWidth: 0, },
            grid: { borderColor: '#3d3d3d' },
            labels: [1, 2, 3, 4, 5, 6],
            xaxis: { 
                tooltip: { enabled: false },
                labels: { style: { colors: chartTextColor, fontFamily: 'Inter' } }
            },
            yaxis: {
                forceNiceScale: true,
                labels: { style: { colors: [chartTextColor], fontFamily: 'Inter' } }
            },
            legend: { position: 'bottom', horizontalAlign: 'center', fontFamily: 'Inter' },
            responsive: [
                {
                    breakpoint: 1000,
                    options: {
                    }
                }
            ]
        }

        let chartLine = new ApexCharts(document.querySelector('#chart'), optionsLine);
        chartLine.render();
        
        this.loaded = true
        /* setTimeout(() => {
            this.loaded = true
        }, 1000) */
    }
}
</script>

