---
title: "Segmentando una red con VLANs en Fedora"
date: 2026-06-12
tags: [networking, linux, vlans]
---

Después de cablear media casa para probar un entorno realista, llegué a la conclusión de que **las VLANs** son la mejor forma de entender subnetting sin volverse loco.

## Setup

Hardware:

- Switch TP-Link TL-SG108E (manageable, capa 2)
- Router con OpenWrt
- 2 notebooks + 1 Raspberry Pi 4 corriendo Fedora Server 41

La idea era simular tres segmentos lógicos sobre el mismo cableado físico:

| VLAN | Nombre       | Subnet           | Gateway       |
| ---- | ------------ | ---------------- | ------------- |
| 10   | `mgmt`       | `10.10.10.0/24`  | `10.10.10.1`  |
| 20   | `clients`    | `10.20.20.0/24`  | `10.20.20.1`  |
| 30   | `iot`        | `10.30.30.0/24`  | `10.30.30.1`  |

## Switch

El TL-SG108E tiene una UI web bastante fea pero funcional. Para cada VLAN:

1. Crear la VLAN con un ID y un nombre.
2. Asignar los puertos en modo *tagged* (trunk) o *untagged* (access).
3. Definir el PVID de cada puerto access según la VLAN a la que pertenece.

El puerto que va al router lo dejé en modo *trunk* para que transporte las tres VLANs.

## Router (OpenWrt)

En `/etc/config/network`:

```sh
config interface 'mgmt'
    option ifname 'eth0.10'
    option proto 'static'
    option ipaddr '10.10.10.1'
    option netmask '255.255.255.0'

config interface 'clients'
    option ifname 'eth0.20'
    option proto 'static'
    option ipaddr '10.20.20.1'
    option netmask '255.255.255.0'

config interface 'iot'
    option ifname 'eth0.30'
    option proto 'static'
    option ipaddr '10.30.30.1'
    option netmask '255.255.255.0'
```

Cada `eth0.X` es la subinterfaz VLAN sobre la interfaz física `eth0`.

## Verificación

Desde la Raspberry Pi:

```sh
$ ip link add link eth0 name eth0.20 type vlan id 20
$ ip addr add 10.20.20.50/24 dev eth0.20
$ ip link set eth0.20 up
$ ping 10.20.20.1
PING 10.20.20.1 (10.20.20.1) 56(84) bytes of data.
64 bytes from 10.20.20.1: icmp_seq=1 ttl=64 time=0.412 ms
```

Y un `tcpdump -nei eth0.20` para confirmar que las tramas llegan con el tag 802.1Q correcto.

## Por qué sirve

Antes de tocar Kubernetes o service meshes, entender VLANs + subinterfaces Linux te da intuición sobre **aislamiento de capa 2** que después se traduce 1:1 a namespaces de red y CNIs. Es la mejor ejercitación de subnetting que conozco.
