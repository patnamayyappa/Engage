import { Component, OnInit } from "@angular/core";
import { InternalConnectionsService } from "./../../services/internal-connections.service"
import { InternalConnection } from "./../../models/internal-connection.model";

@Component({
  selector: "app-internal-connections",
  templateUrl: "./internal-connections.component.html",
  styleUrls: ["./internal-connections.component.less"]
})
export class InternalConnectionsComponent implements OnInit {
  networkDetails: any;
  isActiveAll: boolean = false;
  private rwindow: any = window;
  private webResourceDir: string = "/../../../../../WebResources/";
  private contactId: string;
  connections: InternalConnection[] = [];
  connectionTypes: any = [{ id: "AllConnection", name: this.rwindow.CampusManagement.localization.getResourceString("AllConnection")},
    { id: "InternalConnection", name: this.rwindow.CampusManagement.localization.getResourceString("InternalConnection")}];
  nodes: any = [];
  edges: any = [];

  constructor(private internalConnectionsService: InternalConnectionsService) {
  }

  getContactNodeImage() {
    return `/../../../../Image/download.aspx?Entity=contact&Attribute=entityimage&Id=${this.contactId}`;
  }

  getTargetNodeImage(connection: InternalConnection): string {
    let imagePath: string = "";

    switch (connection.targetType) {
      case "account":
        imagePath = `/../../../Image/download.aspx?Entity=account&Attribute=entityimage&Id={${connection.targetGuid}}`;
        break;
      case "contact":
        imagePath = `/../../../Image/download.aspx?Entity=contact&Attribute=entityimage&Id={${connection.targetGuid}}`;
        break;
      default:
        imagePath = "CampusBrokenImage.png";
        break;
    }
    return imagePath;
  }

  renderNetwork(results, _this) {
    var that = _this;
    that.nodes = [];
    that.edges = [];
    that.connections = [];
    console.log("Insider render network");
    //Fetch the connections from the results
    results["0"].entities.forEach(function (connection) {
      that.connections.push({
        connectionType: connection["connectionRole.name"],
        connectionRoleGuid: connection["_record2roleid_value"],
        targetGuid: connection["_record2id_value"],
        targetName: connection["_record2id_value@OData.Community.Display.V1.FormattedValue"],
        targetType: connection["_record2id_value@Microsoft.Dynamics.CRM.lookuplogicalname"]
      });
    });

    const mapOfNodeGuidToIndex = {};
    const mapOfSourceEdges = {};

    let contactName = (that.rwindow.parent.Xrm.Page.getAttribute("firstname").getValue() || "") + " " + (that.rwindow.parent.Xrm.Page.getAttribute("lastname").getValue() || "")
    //Add the Contact node first
    that.nodes.push({
      id: 1,
      label: contactName,
      group: "source",

    });

    //Loop through each connection and prepare the related nodes and their edges
    let nodeIndex: number = 2;
    for (let i = 0; i < that.connections.length; i++) {
      const connection = that.connections[i];
      if (mapOfNodeGuidToIndex[connection.connectionRoleGuid] === undefined) {
        mapOfNodeGuidToIndex[connection.connectionRoleGuid] = nodeIndex;
        //Add the connection Node
        that.nodes.push({
          id: nodeIndex,
          label: connection.connectionType,
          group: "connection"
        });
        nodeIndex = nodeIndex + 1;
      }
      if (mapOfNodeGuidToIndex[connection.targetGuid] === undefined) {
        mapOfNodeGuidToIndex[connection.targetGuid] = nodeIndex;
        //Add the connection Node
        that.nodes.push({
          id: nodeIndex,
          image: that.getTargetNodeImage(connection),
          label: connection.targetName,
          group: "target",
          brokenImage: connection.targetType === "account" ? this.webResourceDir + "cmc_CampusBrokenImage.png" : this.webResourceDir + "cmc_ContactBrokenImage.png",
          color: { border: connection.targetType === "account" ? "orange" : "green" }
        });
        nodeIndex = nodeIndex + 1;
      }
      if (mapOfSourceEdges[mapOfNodeGuidToIndex[connection.connectionRoleGuid]] === undefined) {
        that.edges.push({
          from: 1,
          to: mapOfNodeGuidToIndex[connection.connectionRoleGuid],
          arrows: "to"
        });
        mapOfSourceEdges[mapOfNodeGuidToIndex[connection.connectionRoleGuid]] = true;
      }

      that.edges.push({
        from: mapOfNodeGuidToIndex[connection.connectionRoleGuid],
        to: mapOfNodeGuidToIndex[connection.targetGuid],
        arrows: "to"
      });
    }

    that.networkDetails = {
      networkData: {
        nodes: that.nodes,
        edges: that.edges
      },
      options: that.getNetworkOptions()
    };
  }

  getNetworkOptions() {
    return {
      nodes: {
        borderWidth: 4,
        size: 20,
        color: {
          border: "#222222",
          background: "#666666"
        },
        font: { color: "#000000" }
      },
      edges: {
        width: 1,
        shadow: true
      },
      groups: {
        source: {
          shape: "circularImage",
          image: this.getContactNodeImage(),
          brokenImage: this.webResourceDir + "cmc_ContactBrokenImage.png",
          color: { border: "blue" },
          size: 30
        },
        connection: {
          size: 10,
          shape: "dot",
          color: {
            background: "#FFFFFF"
          },
          borderWidth: 1
        },
        target: {
          shape: "circularImage"

        }
      }
    };
  }

  ngOnInit() {   
    this.contactId = this.rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    this.internalConnectionsService.getConnections("InternalConnection").then(results => { this.renderNetwork(results, this) });
    this.isActiveAll = false;

  }
  onClick(input) {
    this.contactId = this.rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    switch (input.toLowerCase()) {
      case "internalconnection": this.internalConnectionsService.getConnections("InternalConnection").then(results => { this.renderNetwork(results, this); this.isActiveAll = false;  }); break;
      case "allconnection": this.internalConnectionsService.getConnections("AllConnection").then(results => { this.renderNetwork(results, this); this.isActiveAll = true; }); break;
    }
   
  }
}
