import { Component, OnInit, NgZone } from '@angular/core';
import { TripMapsService } from './../../services/trip-maps.service';


@Component({
  selector: 'app-trip-maps',
  templateUrl: './trip-maps.component.html',
  styleUrls: ['./trip-maps.component.less']
})
export class TripMapsComponent implements OnInit {
  public pinInfobox: any;
  public tooltip: any;
  public tooltipTemplate: any;
  constructor(private zone: NgZone, private _tripService: TripMapsService) {
    let xWindow: any = window;
    xWindow.angularComponentRef = {
      zone: this.zone,
      componentFn: () => this.getBingMap(),
      component: this
    };
  }

  ngOnInit() {
    this.loadScripts();
  }

  public getBingMap(): void {
    let Microsoft: any;
    let xWindow: any = window;
    enum cmc_activitytype {
      Appointment = 175490000,
      Event = 175490001
    }
    this.tooltip;
    let tripActivities: any;
    this._tripService.getBingMapKey().subscribe((result: any) => {
      let binMapKey = result;
      let map = new xWindow.Microsoft.Maps.Map(document.getElementById('bingMap'), {
        credentials: binMapKey
      });
      this._tripService.getData().subscribe(
        (result: any) => {
          tripActivities = result[0].value;
          if (tripActivities.length > 0) {
            let map = new xWindow.Microsoft.Maps.Map(document.getElementById('bingMap'), {
              credentials: binMapKey
            });
            let pushPinInfos = [];

            let pinLayer = new xWindow.Microsoft.Maps.EntityCollection();
            let locs = [];

            this.tooltip = new xWindow.Microsoft.Maps.Infobox(map.getCenter(), {
              visible: false,
              showPointer: false,
              showCloseButton: false,
              offset: new xWindow.Microsoft.Maps.Point(-75, 35)
            });
            this.tooltip.setMap(map);

            // Create the info box for the pushpin
            this.pinInfobox = new xWindow.Microsoft.Maps.Infobox(map.getCenter(), {
              visible: false
            });

            this.pinInfobox.setMap(map);

            for (let i = 0, j = 0; i < tripActivities.length; i++) {
              if ((tripActivities[i].cmc_tripactivitylatitude !== null) && (tripActivities[i].cmc_tripactivitylongitude !== null)) {
                let pin: any;
                locs[j] = new xWindow.Microsoft.Maps.Location(tripActivities[i].cmc_tripactivitylatitude, tripActivities[i].cmc_tripactivitylongitude);
                if (tripActivities[i].cmc_activitytype === cmc_activitytype.Appointment) {
                  pin = new xWindow.Microsoft.Maps.Pushpin(locs[j], { icon: '../../WebComponents/assets/images/appointmentPushPin.svg', text: (i + 1).toString() });
                }
                else if (tripActivities[i].cmc_activitytype === cmc_activitytype.Event) {
                  pin = new xWindow.Microsoft.Maps.Pushpin(locs[j], { icon: '../../WebComponents/assets/images/eventPushPin.svg', text: (i + 1).toString() });
                }
                else {
                  pin = new xWindow.Microsoft.Maps.Pushpin(locs[j], { icon: '../../WebComponents/assets/images/othersPushPin.svg', text: (i + 1).toString() });
                }
                pin.metadata = {
                  title: tripActivities[i].cmc_name,
                  description: '<div class="">' + tripActivities[i].cmc_location + '<div class="bingMapNoWrap">Start Date :' + new Date(tripActivities[i].cmc_startdatetime).toLocaleString() + '</div><div class="bingMapNoWrap"> End Time :' + new Date(tripActivities[i].cmc_enddatetime).toLocaleString() + '</div></div>'
                };

                pin.Title = tripActivities[i].cmc_name;
                pinLayer.push(pin);
                xWindow.Microsoft.Maps.Events.addHandler(pin, 'click', this.pushpinClicked.bind(this));
                xWindow.Microsoft.Maps.Events.addHandler(pin, 'mouseover', this.pushpinHovered.bind(this));
                xWindow.Microsoft.Maps.Events.addHandler(pin, 'mouseout', this.closeTooltip.bind(this));
                j++;
              }
            }
            map.layers.insert(pinLayer);
            let line = new xWindow.Microsoft.Maps.Polyline(locs);
            let bestview = xWindow.Microsoft.Maps.LocationRect.fromLocations(locs);
            map.setView({
              center: bestview.center,
              bounds: bestview
            });
            map.entities.push(line);
          }
          else {
            map.setView({
              center: map.getCenter(),
              zoom: map.getZoom()
            });
          }
        }
      )
    })

  }

  public pushpinClicked(e) {
    let that = this;
    //Make sure the infobox has metadata to display.
    if (e.target.metadata) {
      //Set the infobox options with the metadata of the pushpin.
      that.pinInfobox.setOptions({
        location: e.target.getLocation(),
        title: e.target.metadata.title,
        description: e.target.metadata.description,
        visible: true
      });
    }
  }
  public pushpinHovered(e) {
    let that = this;
    //Hide the infobox
    that.hideInfobox(e);
    //Make sure the infobox has metadata to display.
    if (e.target) {
      //Set the infobox options with the metadata of the pushpin.
      that.tooltip.setOptions({
        location: e.target.getLocation(),
        htmlContent: '<div class="bingMapInfobox" >' + e.target.Title + '</div>',
        visible: true
      });
    }
  }

  public hideInfobox(e) {
    let that = this;
    that.pinInfobox.setOptions({
      visible: false
    });
  }
  public closeTooltip() {
    let that = this;
    //Close the tooltip and clear its content.
    that.tooltip.setOptions({
      visible: false
    });
  }
  loadScripts() {
    const dynamicScripts = ['https://www.bing.com/api/maps/mapcontrol?callback=getBingMap'];
    for (let i = 0; i < dynamicScripts.length; i++) {
      const node = document.createElement('script');
      node.src = dynamicScripts[i];
      node.type = 'text/javascript';
      node.async = false;
      node.defer = false;
      node.charset = 'utf-8';
      document.getElementsByTagName('head')[0].appendChild(node);

    }
  }
}
