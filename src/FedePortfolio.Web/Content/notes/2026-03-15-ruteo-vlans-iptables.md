---
title: "Ruteo entre VLANs con iptables: cuándo nftables se queda corto"
date: 2026-03-15
tags: [linux, networking, iptables, routing]
---

Por defecto, dos VLANs en un router Linux están **aisladas**. El tráfico entre ellas se descarta porque el kernel, en el momento en que el paquete cruza el *routing decision*, decide que pertenece a una interfaz diferente y lo bloquea si no hay forwarding explícito.

## El setup mínimo

```sh
# Habilitar forwarding global
$ sysctl -w net.ipv4.ip_forward=1

# Verificar que las interfaces existen
$ ip -br link show | grep vlan
eth0.10  UP    10.10.10.1/24
eth0.20  UP    10.20.20.1/24
```

Hasta acá, sin reglas, las VLANs se ven a sí mismas pero no a las otras.

## La trampa del `conntrack`

Si solamente abrís el forwarding con `iptables -A FORWARD -j ACCEPT`, los hosts de ambas VLANs se ven entre sí, pero **no hay NAT ni filtrado de capa 4**. Cualquiera puede hablar cualquier protocolo en cualquier puerto.

Para algo más serio:

```sh
# Stateful: dejá pasar tráfico de conexiones establecidas
$ iptables -A FORWARD -m conntrack --ctstate ESTABLISHED,RELATED -j ACCEPT

# Permitir SSH de mgmt hacia clients
$ iptables -A FORWARD -i eth0.10 -o eth0.20 -p tcp --dport 22 \
    -m conntrack --ctstate NEW -j ACCEPT

# Drop explícito del resto inter-VLAN
$ iptables -A FORWARD -i eth0.10 -o eth0.20 -j DROP
$ iptables -A FORWARD -i eth0.20 -o eth0.10 -j DROP
```

## Por qué nftables a veces se queda corto

`nftables` es la evolución natural y reemplaza a `iptables` con un solo backend. Pero hay dos casos donde seguí prefiriendo `iptables` (o más bien su sucesor `nft` con sintaxis legacy compatible):

1. **Reglas con `conntrack` muy granulares**: la sintaxis de `nft` es más limpia, pero cuando necesito expresiones de matching complejas con `recent`, `time` o `hashlimit`, `iptables` sigue siendo más directo de escribir.
2. **Compatibilidad con herramientas de terceros**: algunos paneles y demonios todavía generan reglas con `iptables-save`. El bridging con `nftables` nativo es posible pero requiere un `iptables-nft` bien configurado.

## Mi flujo de trabajo

1. **Pruebo en `nftables`** con sintaxis moderna. La mayoría de las reglas son 30% más cortas.
2. **Si aparece algo raro** (matching de tiempo, rate limiting por IP, geoip) salto a `iptables-nft` para no reinventar la rueda.
3. **Persisto siempre con `iptables-save` / `iptables-restore`** en `/etc/iptables/rules.v4` para que sobrevivan reboots.

El próximo paso en este setup va a ser migrar a `nft` puro y ver si pierdo algo en el camino.
