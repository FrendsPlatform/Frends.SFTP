FROM atmoz/sftp

ARG DEBIAN_FRONTEND=noninteractive

COPY ./Volumes/sshd_config /etc/ssh/sshd_config

ENTRYPOINT ["tail", "-f", "/dev/null"]