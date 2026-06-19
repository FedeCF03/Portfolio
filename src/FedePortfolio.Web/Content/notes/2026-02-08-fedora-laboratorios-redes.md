---
title: "Por qué prefiero Fedora para mis laboratorios de redes"
date: 2026-02-08
tags: [linux, fedora, networking]
---

Después de pasar por Debian, Ubuntu Server, Arch y openSUSE, terminé volviendo siempre a **Fedora** para mis laboratorios de redes. No es una decisión dogmática: es práctica.

## Lo que me sirve

### 1. NetworkManager con `nmcli` es predecible

Las distros modernas pelean entre NetworkManager, `systemd-networkd` y scripts varios. En Fedora, **NetworkManager gana** y `nmcli` es la API canónica. Puedo versionar mis conexiones:

```sh
$ nmcli connection export "mgmt-vlan10" /etc/nm-mgmt-vlan10.nmconnection
```

Y después replicarlas con un solo import.

### 2. Kernel siempre nuevo

Fedora usa kernels muy recientes. Para experimentar con:

- `tc` y qdisc para traffic shaping
- eBPF para observabilidad
- WireGuard y submódulos de crypto
- `xdp` con drivers reales

…necesito un kernel que no tenga seis meses. Fedora cumple.

### 3. SELinux, pero sin sorpresas

Sí, SELinux rompe cosas si no sabés. Pero una vez que entendés el modelo, te salva de accidentes tontos en entornos multi-tenant. Y los mensajes de `audit2why` / `audit2allow` son lo suficientemente claros como para resolver en minutos.

## Lo que me complica

- **Ciclo de release rápido**: 6 meses entre versiones mayor y 1 año de soporte. Hay que actualizar seguido.
- **dnf es más lento que apt** en mi experiencia, sobre todo en actualizaciones grandes.
- **Repos no oficiales más limitados** que en Debian/Ubuntu (aunque RPM Fusion es excelente).

## Setup típico de mi laboratorio

```sh
# Después de una Fedora Server minimal
$ sudo dnf install -y \
    NetworkManager-tui \
    nm-connection-editor \
    tcpdump wireshark-cli \
    iptables-nft nftables \
    bind-utils \
    iproute \
    python3-pip

# Habilitar forwarding y desactivar cosas que no necesito
$ sudo systemctl disable --now firewalld
$ sudo systemctl enable --now nftables
```

Para mis proyectos .NET, Fedora viene con `dotnet` 10 en los repos oficiales, así que no tengo que pelearme con el `dotnet-install.sh`.

## Conclusión

Si tu prioridad es **estudiar redes de verdad** y no batallar con la distro, Fedora es la opción menos opinada. Ubuntu es más fácil al principio, Arch te enseña más sobre el sistema, pero Fedora te deja trabajar.
