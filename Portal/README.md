# Portals

## Developing for Portals
- VS Code is the recommended editor.
  - Install the Liquid Language Support extension for Liquid syntax highlighting.
  - When creating liquid templates, save the file with the .liquid file extension.

### Folder Structure

#### Assets
Contains any global assets, e.g., images, stylesheets, JavaScript.

#### Vendor
Contains 3rd party components, e.g., Bootstrap source.

#### Web Pages
Save any custom HTML used on specific Portal Web Pages. This includes any related script and/or styles. For instance, if there is a portal web page for appointments that contains custom HTML, JS, and CSS:
- appointments.css
- appointments.html
- appointments.js

#### Web Templates
Save any Liquid Web Templates here. This includes OOB templates that have been modified as well as any new/custom templates.
- calendar-template.liquid

## Resources
- [Portal KB](https://kb.sonomapartners.com/display/DC/Portals)
- [Adxstudio Liquid Docs](https://community.adxstudio.com/products/adxstudio-portals/documentation/configuration-guide/liquid-templates/)
- [Dynamics 365 Portals Liquid Docs](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/portals/custom-templates-dynamic-content)
- [Dynamics 365 portals: Use liquid to return JSON or XML](https://community.dynamics.com/enterprise/b/colinvermandermicrosoft/archive/2017/04/17/dynamics-365-portals-use-liquid-to-return-json-or-xml)
